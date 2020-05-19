using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Printing;
using System.Printing.Interop;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO; 
using System.Windows.Documents.Serialization;
using System.Windows.Xps;
using Utils.Common;
using Utils.Helps;

namespace Utils.Print
{
    public enum PageOrientation
    {
        Landscape,
        Portrait
    }

    public class PrintHelper
    {
        private XpsDocumentWriter _xpsDocWriter = null;
        private PrintDocPaginator _docPaginator = null;
        
        private FrameworkElement _printedElement = null;
        private Transform _originalTransform = null;
        private Size? _originalSize = null;

        private Action<WritingCompletedEventArgs> _printCompleted = null;
        private Action<WritingProgressChangedEventArgs> _progressChanged = null;

        private static readonly PrintHelper _instance = new PrintHelper();

        private PrintHelper()
        { }

        public static PrintHelper Instance()
        {
            return _instance;
        }

        public bool Print(string printerName, string ticketSetting, int pageCount,
            Func<Size, PageOrientation> getPageOrientation, 
            Func<int, int, Size, Visual> getPage, 
            Action<WritingProgressChangedEventArgs> progressChanged,
            Action<WritingCompletedEventArgs> printCompleted)
        {
            if (string.IsNullOrEmpty(printerName))
                return false;

            using (var printer = GetPrinter(printerName))
            {
                if (printer == null)
                    return false;

                var printTicket = GetPrintTicket(printer, ticketSetting);

                /*The first way
                var printDialog = new PrintDialog();
                printDialog.PrintQueue = printer;
                printDialog.PrintTicket = printTicket;
                */

                //The second way
                printer.UserPrintTicket = printTicket;

                //Page Size  1 inch = 96 pixels
                Size originalPageSize;
                try
                {
                    var pageImageableArea = printer.GetPrintCapabilities(printTicket).PageImageableArea;
                    originalPageSize = new Size(pageImageableArea.ExtentWidth, pageImageableArea.ExtentHeight);
                }
                catch
                {
                    originalPageSize = new Size(printTicket.PageMediaSize.Width.Value, printTicket.PageMediaSize.Height.Value);
                }

                //Page Orientation
                if (getPageOrientation != null)
                {
                    if (getPageOrientation(originalPageSize) == PageOrientation.Landscape)
                    {
                        //exchange width and height
                        originalPageSize = new Size(originalPageSize.Height, originalPageSize.Width);
                        printTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                    }
                    else
                        printTicket.PageOrientation = System.Printing.PageOrientation.Portrait;
                }
                else
                {
                    if (printTicket.PageOrientation == System.Printing.PageOrientation.Landscape)
                        originalPageSize = new Size(originalPageSize.Height, originalPageSize.Width);
                }

                try
                {
                    //The first way
                    //printDialog.PrintDocument(new LabelDocPaginator(pageCount, originalPageSize, getSpecifiedPage), "Print Label");

                    //The second way
                    DisposeXpsDocWriter();

                    _printCompleted = printCompleted;
                    _progressChanged = progressChanged;

                    _xpsDocWriter = PrintQueue.CreateXpsDocumentWriter(printer);
                    _xpsDocWriter.WritingCompleted += WritingCompleted;
                    _xpsDocWriter.WritingProgressChanged += WritingProgressChanged;

                    _docPaginator = new PrintDocPaginator(pageCount, originalPageSize, getPage);
                    _xpsDocWriter.WriteAsync(_docPaginator);

                }
                catch (Exception ex)
                {
                    DisposeXpsDocWriter();

                    _printCompleted = null;
                    _progressChanged = null;

                    return false;
                }
            }

            return true;
        }

        public void Cancel()
        {
            if (_xpsDocWriter != null)
                _xpsDocWriter.CancelAsync();
        }

        public List<string> GetAllPrinters()
        {
            using (var printServer = new PrintServer())
            {
                using (var printQueues = printServer.GetPrintQueues())
                {
                    return printQueues.Select(p => p.FullName).ToList();
                }
            }
        }

        public PageOrientation AutoComputePageOrientation(Size elementSize, Size pageSize)
        {
            var elementRatio = elementSize.Width / elementSize.Height;
            var pageRatio = pageSize.Width / pageSize.Height;

            if (DoubleUtil.GreaterThan(elementRatio, 1) && DoubleUtil.LessThan(pageRatio, 1)
                || DoubleUtil.LessThan(elementRatio, 1) && DoubleUtil.GreaterThan(pageRatio, 1))
                return PageOrientation.Landscape;

            return PageOrientation.Portrait;
        }

        public Visual GetSpecifiedPage(Brush brush, Size elementSize, Size pageSize, bool isFill)
        {
            var printSize = elementSize;

            if (isFill)
            {
                var elementRatio = elementSize.Width / elementSize.Height;
                var pageRatio = pageSize.Width / pageSize.Height;

                if (DoubleUtil.LessThan(elementRatio, pageRatio))
                    printSize = new Size(pageSize.Height / elementSize.Height * elementSize.Width, pageSize.Height);
                else
                    printSize = new Size(pageSize.Width, pageSize.Width / elementSize.Width * elementSize.Height);
            }

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(
                    brush,
                    null,
                    new Rect(
                        Math.Max(0, pageSize.Width - printSize.Width) / 2,
                        Math.Max(0, pageSize.Height - printSize.Height) / 2,
                        printSize.Width, printSize.Height));
            }

            return dv;
        }

        public Visual GetSpecifiedPage(FrameworkElement element, Size pageSize, bool fillToPage)
        {
            var elementSize = new Size(element.ActualWidth, element.ActualHeight);
            var printSize = elementSize;

            if (fillToPage)
            {
                var elementRatio = elementSize.Width / elementSize.Height;
                var pageRatio = pageSize.Width / pageSize.Height;

                if (DoubleUtil.LessThan(elementRatio, pageRatio))
                    printSize = new Size(pageSize.Height / elementSize.Height * elementSize.Width, pageSize.Height);
                else
                    printSize = new Size(pageSize.Width, pageSize.Width / elementSize.Width * elementSize.Height);
                
                if (!DoubleUtil.AreClose(printSize.Width, elementSize.Width) || !DoubleUtil.AreClose(printSize.Height, elementSize.Height))
                {
                    //for restore
                    _printedElement = element;
                    _originalTransform = element.LayoutTransform;
                    _originalSize = elementSize;

                    Resize(element,
                        new ScaleTransform(printSize.Width / elementSize.Width, printSize.Height / elementSize.Height),
                        new Rect(
                            Math.Max(0, pageSize.Width - printSize.Width) / 2,
                            Math.Max(0, pageSize.Height - printSize.Height) / 2,
                            printSize.Width, printSize.Height));
                }
            }
            
            return element;

            /*
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var contentBounds = VisualTreeHelper.GetDescendantBounds(element);
                dc.DrawRectangle(
                    new VisualBrush(element) { Stretch = Stretch.Fill, Viewbox = new Rect(0, 0, elementSize.Width / contentBounds.Width, elementSize.Height / contentBounds.Height) },
                    null,
                    new Rect(
                        Math.Max(0, pageSize.Width - printSize.Width) / 2,
                        Math.Max(0, pageSize.Height - printSize.Height) / 2,
                        printSize.Width, printSize.Height));
            }

            return dv;
            */
        }

        public string OpenPrinterProperties(Window window, string printerName, string printTicketStr)
        {
            if (string.IsNullOrEmpty(printerName))
                return null;

            using (var printer = GetPrinter(printerName))
            {
                if (printer == null)
                    return null;

                PrintTicket printTicket = null;

                if (string.IsNullOrEmpty(printTicketStr))
                    printTicket = printer.DefaultPrintTicket;
                else
                {
                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            using (var sw = new StreamWriter(ms))
                            {
                                sw.Write(printTicketStr);
                                sw.Flush();

                                ms.Position = 0;
                                printTicket = new PrintTicket(ms);
                            }
                        }
                    }
                    catch
                    {
                        printTicket = printer.DefaultPrintTicket;
                    }
                }

                using (var result = OpenPrinterProperties(window, printer, printTicket).GetXmlStream())
                {
                    result.Position = 0;
                    using (var sr = new StreamReader(result))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        public Stream OpenPrinterProperties(Window window, string printerName, Stream ticketStream)
        {
            if (string.IsNullOrEmpty(printerName))
                return null;

            using (var printer = GetPrinter(printerName))
            {
                if (printer == null)
                    return null;

                var printTicket = OpenPrinterProperties(window, printer, ticketStream == null ? printer.DefaultPrintTicket : new PrintTicket(ticketStream));
                return printTicket.GetXmlStream();
            }
        }

        private PrintTicket OpenPrinterProperties(Window window, PrintQueue printer, PrintTicket ticket)
        {
            var ptc = new PrintTicketConverter(printer.FullName, printer.ClientPrintSchemaVersion);
            var mainWindowPtr = new WindowInteropHelper(window).Handle;

            var devMode = ptc.ConvertPrintTicketToDevMode(ticket, BaseDevModeType.UserDefault);

            var pinnedDevMode = GCHandle.Alloc(devMode, GCHandleType.Pinned);
            var pDevMode = pinnedDevMode.AddrOfPinnedObject();

            Win32.DocumentProperties(mainWindowPtr, IntPtr.Zero, printer.FullName, pDevMode, pDevMode, 14);

            var newTicket = ptc.ConvertDevModeToPrintTicket(devMode);
            pinnedDevMode.Free();

            return newTicket;
        }

        private PrintTicket GetPrintTicket(PrintQueue printer, string ticketSetting)
        {
            if (string.IsNullOrEmpty(ticketSetting))
                return printer.DefaultPrintTicket;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.Write(ticketSetting);
                    sw.Flush();

                    ms.Position = 0;
                    return new PrintTicket(ms);
                }
            }
        }

        private PrintQueue GetPrinter(string printerName)
        {
            if (string.IsNullOrEmpty(printerName))
                return LocalPrintServer.GetDefaultPrintQueue();

            using (var printServer = new PrintServer())
            {
                using (var printQueues = printServer.GetPrintQueues())
                {
                    var printer = printQueues.FirstOrDefault(p => p.FullName == printerName);
                    if (printer != null)
                        return printer;
                }
            }

            return LocalPrintServer.GetDefaultPrintQueue();
        }

        private void WritingCompleted(object sender, WritingCompletedEventArgs e)
        {
            DisposeXpsDocWriter();

            if (_printCompleted != null)
                _printCompleted(e);
        }

        private void WritingProgressChanged(object sender, WritingProgressChangedEventArgs e)
        {
            if (e.WritingLevel == WritingProgressChangeLevel.FixedPageWritingProgress)
            {
                if (_printedElement != null && _originalTransform != null && _originalSize != null)
                    Resize(_printedElement, _originalTransform, new Rect(new Point(), _originalSize.Value));

                if (_progressChanged != null)
                    _progressChanged(e);
            }
        }

        private void Resize(FrameworkElement element, Transform transform, Rect finalRect)
        {
            element.LayoutTransform = transform;
            element.Measure(finalRect.Size);
            element.Arrange(finalRect);
        }
        
        private void DisposeXpsDocWriter()
        {
            if (_xpsDocWriter != null)
            {
                _xpsDocWriter.WritingCompleted -= WritingCompleted;
                _xpsDocWriter.WritingProgressChanged -= WritingProgressChanged;

                _xpsDocWriter = null;
            }

            if (_docPaginator != null)
            {
                _docPaginator.Dispose();
                _docPaginator = null;
            }
            
            _printedElement = null;
            _originalTransform = null;
            _originalSize = null;
        }
    }
}


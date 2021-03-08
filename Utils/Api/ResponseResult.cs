using System;

namespace CASApp.Service.API.Response
{
    /// <summary>
    /// Web Api 消息类基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ResponseResult<T>
    {
        bool Success { get; }
        string Message { get; }
        T Content { get; }
    }

    /// <summary>
    /// CAS WebApi 返回值的消息类
    /// </summary>
    /// <typeparam name="T">具体的返回数据类型</typeparam>
    public class CAS_ResponseResult<T> : ResponseResult<T>
    {
        public bool Success => code == "0";
        public string Message => mesg;
        public T Content => data;

        public string code { get; set; }
        public string mesg { get; set; }

        public T data { get; set; }
    }

    /// <summary>
    /// MP WebApi 返回值的消息类
    /// </summary>
    /// <typeparam name="T">具体的返回数据类型</typeparam>
    public class MP_ResponseResult<T> : ResponseResult<T>
    {
        public bool Success => Convert.ToBoolean(state);
        //[JsonIgnore]
        public string Message => message;
        //[JsonIgnore]
        public T Content => content;

        public string state { get; set; } // true/false
        public string method { get; set; }
        public string message { get; set; }

        public T content { get; set; }
    }
}

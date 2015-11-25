﻿using System;

namespace 个人信息数据库principalComputer.model
{
    [Serializable]
    public class caddressBook
    {
        public caddressBook()
        {

        }
        /// <summary>
        /// 标识符
        /// </summary>
        public string id
        {
            set;
            get;
        }
        /// <summary>
        /// 通讯人姓名
        /// </summary>
        public string name
        {
            set;
            get;
        }
        /// <summary>
        /// 联系方式
        /// </summary>
        public string contact
        {
            set;
            get;
        }
        /// <summary>
        /// 工作地点
        /// </summary>
        public string address
        {
            set;
            get;
        }
        /// <summary>
         /// 城市
         /// </summary>
        public string city
        {
            set;
            get;
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string comment
        {
            set;
            get;
        }
    }
}

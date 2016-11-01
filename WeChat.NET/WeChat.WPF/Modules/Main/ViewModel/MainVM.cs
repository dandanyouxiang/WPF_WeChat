﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using WeChat.HTTP;
using WeChat.WPF.Modules.Main.Model;
using System.Collections.ObjectModel;

namespace WeChat.WPF.Modules.Main.ViewModel
{
    public class MainVM : ViewModelBase
    {
        WeChatService wcs = new WeChatService();

        public MainVM()
        {
            Init();
        }

        #region 字段属性
        /// <summary>
        /// 当前登录微信用户
        /// </summary>
        private WeChatUser _me;
        /// <summary>
        /// 当前登录微信用户
        /// </summary>
        public WeChatUser Me
        {
            get
            {
                return _me;
            }

            set
            {
                _me = value;
                RaisePropertyChanged("Me");
            }
        }

        private WeChatUser _friendUser;
        /// <summary>
        ///  聊天好友
        /// </summary>
        public WeChatUser FriendUser
        {
            get
            {
                return _friendUser;
            }

            set
            {
                if (value != _friendUser)
                {
                    if (_friendUser != null)
                    {
                        _friendUser.MsgRecved -= new WeChatUser.MsgRecvedEventHandler(_friendUser_MsgRecved);
                        _friendUser.MsgSent -= new WeChatUser.MsgSentEventHandler(_friendUser_MsgSent);
                    }
                    _friendUser = value;
                    if (_friendUser != null)
                    {
                        _friendUser.MsgRecved += new WeChatUser.MsgRecvedEventHandler(_friendUser_MsgRecved);
                        _friendUser.MsgSent += new WeChatUser.MsgSentEventHandler(_friendUser_MsgSent);
                        IEnumerable<KeyValuePair<DateTime, WeChatMsg>> dic = _friendUser.RecvedMsg.Concat(_friendUser.SentMsg);
                        dic = dic.OrderBy(p => p.Key);
                        foreach (KeyValuePair<DateTime, WeChatMsg> p in dic)
                        {
                            if (p.Value.From == _friendUser.UserName)
                            {
                                ShowReceiveMsg(p.Value);
                            }
                            else
                            {
                                ShowSendMsg(p.Value);
                            }
                            p.Value.Readed = true;
                        }
                    }
                }
                RaisePropertyChanged("FriendUser");
            }
        }
        /// <summary>
        /// 所有好友列表
        /// </summary>
        private ObservableCollection<object> _contact_all = new ObservableCollection<object>();
        /// <summary>
        /// 通讯录
        /// </summary>
        public ObservableCollection<object> Contact_all
        {
            get
            {
                return _contact_all;
            }

            set
            {
                _contact_all = value;
                RaisePropertyChanged("Contact_all");
            }
        }
        /// <summary>
        /// 部分好友列表
        /// </summary>
        private ObservableCollection<object> _contact_latest = new ObservableCollection<object>();
        /// <summary>
        /// 最近联系人
        /// </summary>
        public ObservableCollection<object> Contact_latest
        {
            get
            {
                return _contact_latest;
            }

            set
            {
                _contact_latest = value;
                RaisePropertyChanged("Contact_latest");
            }
        }

        private object _select_Contact_latest = new object();
        /// <summary>
        /// 聊天列表选中
        /// </summary>
        public object Select_Contact_latest
        {
            get
            {
                return _select_Contact_latest;
            }

            set
            {
                _select_Contact_latest = value;
                RaisePropertyChanged("Select_Contact_latest");
            }
        }

        private string _userName = string.Empty;
        /// <summary>
        /// 用于在顶部显示用户名
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }

            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        private ObservableCollection<ChatMsg> chatList = new ObservableCollection<ChatMsg>();
        /// <summary>
        /// 聊天列表
        /// </summary>
        public ObservableCollection<ChatMsg> ChatList
        {
            get
            {
                return chatList;
            }

            set
            {
                chatList = value;
                RaisePropertyChanged("ChatList");
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            //初始化
            JObject init_result = wcs.WeChatInit();

            List<object> contact_all = new List<object>();
            if (init_result != null)
            {
                _me = new WeChatUser();
                _me.UserName = init_result["User"]["UserName"].ToString();
                _me.City = "";
                _me.HeadImgUrl = init_result["User"]["HeadImgUrl"].ToString();
                _me.NickName = init_result["User"]["NickName"].ToString();
                _me.Province = "";
                _me.PyQuanPin = init_result["User"]["PYQuanPin"].ToString();
                _me.RemarkName = init_result["User"]["RemarkName"].ToString();
                _me.RemarkPYQuanPin = init_result["User"]["RemarkPYQuanPin"].ToString();
                _me.Sex = init_result["User"]["Sex"].ToString();
                _me.Signature = init_result["User"]["Signature"].ToString();
                _me.Icon = GetIcon(wcs, _me.UserName);
                //部分好友名单
                foreach (JObject contact in init_result["ContactList"])
                {
                    WeChatUser user = new WeChatUser();
                    user.UserName = contact["UserName"].ToString();
                    user.City = contact["City"].ToString();
                    user.HeadImgUrl = contact["HeadImgUrl"].ToString();
                    user.NickName = contact["NickName"].ToString();
                    user.Province = contact["Province"].ToString();
                    user.PyQuanPin = contact["PYQuanPin"].ToString();
                    user.RemarkName = contact["RemarkName"].ToString();
                    user.RemarkPYQuanPin = contact["RemarkPYQuanPin"].ToString();
                    user.Sex = contact["Sex"].ToString();
                    user.Signature = contact["Signature"].ToString();
                    user.Icon = GetIcon(wcs, user.UserName);
                    user.SnsFlag = contact["SnsFlag"].ToString();
                    user.KeyWord = contact["KeyWord"].ToString();
                    _contact_latest.Add(user);
                }
            }
            //通讯录
            JObject contact_result = wcs.GetContact();
            if (contact_all != null)
            {
                foreach (JObject contact in contact_result["MemberList"])  //完整好友名单
                {
                    WeChatUser user = new WeChatUser();
                    user.UserName = contact["UserName"].ToString();
                    user.City = contact["City"].ToString();
                    user.HeadImgUrl = contact["HeadImgUrl"].ToString();
                    user.NickName = contact["NickName"].ToString();
                    user.Province = contact["Province"].ToString();
                    user.PyQuanPin = contact["PYQuanPin"].ToString();
                    user.RemarkName = contact["RemarkName"].ToString();
                    user.RemarkPYQuanPin = contact["RemarkPYQuanPin"].ToString();
                    user.Sex = contact["Sex"].ToString();
                    user.Signature = contact["Signature"].ToString();
                    user.Icon = GetIcon(wcs, user.UserName);
                    user.SnsFlag = contact["SnsFlag"].ToString();
                    user.KeyWord = contact["KeyWord"].ToString();
                    user.StartChar = GetStartChar(user);
                    contact_all.Add(user);
                }
            }

            IOrderedEnumerable<object> list_all = contact_all.OrderBy(p => (p as WeChatUser).StartChar).ThenBy(p => (p as WeChatUser).ShowPinYin.Substring(0, 1));

            WeChatUser wcu;
            string start_char;
            foreach (object o in list_all)
            {
                wcu = o as WeChatUser;
                start_char = wcu.StartChar;
                if (!_contact_all.Contains(start_char.ToUpper()))
                {
                    _contact_all.Add(start_char.ToUpper());
                }
                _contact_all.Add(o);
            }
        }
        /// <summary>
        /// 获取头像
        /// </summary>
        /// <param name="wcs"></param>
        /// <param name="_userName"></param>
        /// <returns></returns>
        private ImageSource GetIcon(WeChatService wcs, string _userName)
        {
            if (string.IsNullOrEmpty(_userName))
            {
                return null;
            }
            ImageSource _icon;
            //讨论组
            if (_userName.Contains("@@"))
            {
                _icon = wcs.GetIcon(_userName, StaticUrl.Url_GetHeadImg);
            }
            //好友
            else if (_userName.Contains("@"))
            {
                _icon = wcs.GetIcon(_userName);
            }
            else
            {
                _icon = wcs.GetIcon(_userName);
            }
            return _icon;
        }
        /// <summary>
        /// 获取分组的头
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GetStartChar(WeChatUser user)
        {
            string start_char;

            if (user.KeyWord == "gh_" && user.SnsFlag.Equals("0") || user.KeyWord == "cmb")//user.KeyWord =="cmb"是招商银行信用卡，实在找不出区别了
            {
                start_char = "公众号";
            }
            else if (user.UserName.Contains("@@") && user.SnsFlag.Equals("0"))
            {
                start_char = "群聊";
            }
            else
            {
                start_char = string.IsNullOrEmpty(user.ShowPinYin) ? string.Empty : user.ShowPinYin.Substring(0, 1);
            }
            return start_char;
        }

        #endregion

        #region 聊天事件
        private RelayCommand _loadedCommand;
        /// <summary>
        /// 载入
        /// </summary>
        public RelayCommand LoadedCommand
        {
            get
            {
                return _loadedCommand ?? (_loadedCommand = new RelayCommand(() =>
                    {
                        Thread listener = new Thread(new ThreadStart(new Action(() =>
                        {
                            string sync_flag = "";
                            JObject sync_result;
                            while (true)
                            {
                                //同步检查
                                sync_flag = wcs.WeChatSyncCheck();
                                if (sync_flag == null)
                                {
                                    continue;
                                }
                                //这里应该判断sync_flag中Selector的值
                                else
                                {
                                    sync_result = wcs.WeChatSync();//进行同步
                                    if (sync_result != null)
                                    {
                                        if (sync_result["AddMsgCount"] != null && sync_result["AddMsgCount"].ToString() != "0")
                                        {
                                            foreach (JObject m in sync_result["AddMsgList"])
                                            {
                                                string from = m["FromUserName"].ToString();
                                                string to = m["ToUserName"].ToString();
                                                string content = m["Content"].ToString();
                                                string type = m["MsgType"].ToString();

                                                WeChatMsg msg = new WeChatMsg();
                                                msg.From = from;
                                                msg.Msg = type == "1" ? content : "请在其他设备上查看消息";//只接受文本消息
                                                msg.Readed = false;
                                                msg.Time = DateTime.Now;
                                                msg.To = to;
                                                msg.Type = int.Parse(type);

                                                if (msg.Type == 51)//屏蔽一些系统数据
                                                {
                                                    continue;
                                                }

                                                Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
                                                {
                                                    WeChatUser user;
                                                    bool exist_latest_contact = false;
                                                    foreach (object u in Contact_latest)
                                                    {
                                                        user = u as WeChatUser;
                                                        if (user != null)
                                                        {
                                                            //接收别人消息
                                                            if (user.UserName == msg.From && msg.To == _me.UserName)
                                                            {
                                                                Contact_latest.Remove(user);
                                                                Contact_latest.Insert(0, user);
                                                                exist_latest_contact = true;
                                                                user.ReceivedMsg(msg);
                                                                break;
                                                            }
                                                            //同步自己在其他设备上发送的消息
                                                            else if (user.UserName == msg.To && msg.From == _me.UserName)
                                                            {
                                                                Contact_latest.Remove(user);
                                                                Contact_latest.Insert(0, user);
                                                                exist_latest_contact = true;
                                                                user.SendMsg(msg, true);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    if (!exist_latest_contact)
                                                    {
                                                        foreach (object o in Contact_all)
                                                        {
                                                            WeChatUser friend = o as WeChatUser;
                                                            if (friend != null && friend.UserName == msg.From && msg.To == _me.UserName)
                                                            {
                                                                Contact_latest.Insert(0, friend);
                                                                friend.ReceivedMsg(msg);
                                                                break;
                                                            }
                                                            if (friend != null && friend.UserName == msg.To && msg.From == _me.UserName)
                                                            {
                                                                Contact_latest.Insert(0, friend);
                                                                friend.SendMsg(msg, true);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                                System.Threading.Thread.Sleep(10);
                            }
                        })));
                        listener.Start();
                    }));
            }
        }

        private RelayCommand _chatCommand;
        /// <summary>
        /// 聊天列表的选中事件
        /// </summary>
        public RelayCommand ChatCommand
        {
            get
            {
                return _chatCommand ?? (_chatCommand = new RelayCommand(() =>
                    {
                        if (Select_Contact_latest is WeChatUser)
                        {
                            UserName = (Select_Contact_latest as WeChatUser).ShowName;

                            ChatMsg chatmsg = new ChatMsg();
                            chatmsg.Image = (Select_Contact_latest as WeChatUser).Icon;
                            chatmsg.Message = "测试";
                            chatmsg.FlowDir = FlowDirection.LeftToRight;
                            ChatList.Add(chatmsg);
                        }
                    }));
            }
        }
        /// <summary>
        /// 表示处理开启聊天事件的方法
        /// </summary>
        /// <param name="user"></param>
        public delegate void StartChatEventHandler(WeChatUser user);
        public event StartChatEventHandler StartChat;

        /// <summary>
        /// 发送消息完成
        /// </summary>
        /// <param name="msg"></param>
        void _friendUser_MsgSent(WeChatMsg msg)
        {
            ShowSendMsg(msg);
        }
        /// <summary>
        /// 收到新消息
        /// </summary>
        /// <param name="msg"></param>
        void _friendUser_MsgRecved(WeChatMsg msg)
        {
            ShowReceiveMsg(msg);
        }

        private void ShowSendMsg(WeChatMsg msg)
        {

        }

        private void ShowReceiveMsg(WeChatMsg msg)
        {
            ChatMsg chatmsg = new ChatMsg();
            chatmsg.Image = (Select_Contact_latest as WeChatUser).Icon;
            chatmsg.Message = msg.Msg;
            chatmsg.FlowDir = FlowDirection.LeftToRight;
            ChatList.Add(chatmsg);
        }
        #endregion
    }
}

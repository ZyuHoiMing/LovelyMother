﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LovelyMother.Uwp.Models.Messages;
using LovelyMother.Uwp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Security.Cryptography;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LovelyMother.Uwp.ViewModels
{
    public class CountDownViewModel : ViewModelBase
    {

        //监听算法进程
        private Thread listenForProceess;


        //监听算法变量 : 音乐uri
        private static string[] musicLocation = { "ms-appx:///Assets/Music/1.mp3", "ms-appx:///Assets/Music/2.mp3",
                                           "ms-appx:///Assets/Music/3.mp3", "ms-appx:///Assets/Music/4.mp3",
                                           "ms-appx:///Assets/Music/5.mp3", "ms-appx:///Assets/Music/6.mp3",
                                           "ms-appx:///Assets/Music/7.mp3" };

        //监听算法变量 : 音乐播放器
        private MediaPlayer mediaPlayer;

        //监听算法辅助标识符 : 'true' - 音乐处于播放状态，'false' - 音乐处于关闭状态
        private bool _ifMusicPlaying { get; set; }

        //监听算法辅助标识符 : 为1时取消监听并重置为0
        private bool _listenFlag { get; set; }

        /// <summary>
        /// 服务调用
        /// </summary>

        private readonly IProcessService _processService;

        private readonly IIdentityService _identityService;

        private readonly IRootNavigationService _rootNavigationService;

        //调用一个读取数据库和服务器黑名单的Service - private变量
        List<Motherlibrary.MyDatabaseContext.BlackListProgress> blackListProgresses;


        /// <summary>
        /// 绑定属性。
        /// </summary>

        private TimeSpan _defaultBegin;
        public TimeSpan DefaultBegin
        {
            get => _defaultBegin;
            set => Set(nameof(DefaultBegin), ref _defaultBegin, value);
        }

        private TimeSpan _defaultend;
        public TimeSpan DefaultEnd
        {
            get => _defaultend;
            set => Set(nameof(DefaultEnd), ref _defaultend, value);
        }

        public int Date;

        public int DefaultTime;

        public string Begin;

        //public User CurrentUser;

        //public Task CurrentTask


        /// <summary>
        /// Command类
        /// </summary>

        //_updateLocalTaskCommand

        //_updateWebTaskCommand

        //_InitLocalTaskCommand

        //_InitWebTaskCommand


        /// <summary>
        /// Message类
        /// </summary>
        
        //开始监听进程
        private void BeginListen()
        {
            if (_listenFlag == false)
            {
                //避免多进程运行造成的不必要CPU与内存占用
                _listenFlag = true;

                //打开黑名单: i = 1 => Delay(10000) / 不打开 : i = 0 => delay(2000)
                do
                {
                    var NewProcess = _processService.IfBlackListProcessExist(blackListProgresses, _processService.GetProcessNow());

                    if (NewProcess == false)
                    {
                        Messenger.Default.Send<PunishWindowMessage>(new PunishWindowMessage() { message = "Stop" });
                        if (_ifMusicPlaying == true)
                        {
                            Messenger.Default.Send<StopPlayingMusic>(new StopPlayingMusic());
                        }
                        Task.Delay(10000).Wait();
                    }
                    else
                    {

                        //弹出新窗口
                        Messenger.Default.Send<PunishWindowMessage>(new PunishWindowMessage() {  message = "Begin" });

                        //设置音量50
                        VolumeControl.ChangeVolumeTotheLevel(0.5);

                        //播放音乐
                        if (_ifMusicPlaying == false)
                        {
                            Messenger.Default.Send<BeginPlayingMusic>(new BeginPlayingMusic());
                        }

                        Task.Delay(2000).Wait();
                    }

                    if (_listenFlag == false)
                    {
                        break;
                    }
                }
                while (true);
            }
        }
        
        

        //取消进程监听
        private void StopListen()
        {
            _listenFlag = false;
        }

        public CountDownViewModel(IProcessService processService, IRootNavigationService rootNavigationService,IIdentityService identityService)
        {
            //进程服务所需变量初始化
            listenForProceess = new Thread(this.BeginListen);
            mediaPlayer = new MediaPlayer();
            _listenFlag = false;
            _ifMusicPlaying = false;
            //进程服务所需service初始化
            _processService = processService;
            _identityService = identityService;
            _rootNavigationService = rootNavigationService;
            //TODO : 网易云音乐测试
            blackListProgresses = new List<Motherlibrary.MyDatabaseContext.BlackListProgress>();
            blackListProgresses.Add(new Motherlibrary.MyDatabaseContext.BlackListProgress()
            {
                FileName = "cloudmusic.exe",
                Type = 3
            });

            //开始监听Message注册
            Messenger.Default.Register<BeginListenMessage>(this, async (message) =>
            {
                listenForProceess.Start();
            });

            //取消监听Message注册
            Messenger.Default.Register<StopListenMessage>(this, (message) =>
            {
                StopListen();
            });

            Messenger.Default.Register<BeginPlayingMusic>(this, (message) =>
            {
                BeginPlaying();
            });

            Messenger.Default.Register<StopPlayingMusic>(this, (message) =>
            {
                StopPlaying();
            });
        }

        private void BeginPlaying()
        {
            //随机歌曲
            int random = (int)(CryptographicBuffer.GenerateRandomNumber() % 7);
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(musicLocation[random]));
            mediaPlayer.Play();
            _ifMusicPlaying = true;
        }

        private void StopPlaying()
        {
            //Dispose() : 释放对象
            mediaPlayer.Pause();
            _ifMusicPlaying = false;
        }


        private bool _navigate;
        public bool Navigate
        {
            get => _navigate;
            set => Set(nameof(Navigate), ref _navigate, value);
        }

        /// <summary>
        ///     跳转命令。
        /// </summary>
        /// 
        private RelayCommand _navigateToLoginCommand;

        public RelayCommand NavigateToLoginCommand =>
            _navigateToLoginCommand ?? (_navigateToLoginCommand = new RelayCommand(() => {

                if (_identityService.GetCurrentUserAsync().ID == -1)
                {
                    _navigate = true;
                    _navigateToLoginCommand.RaiseCanExecuteChanged();
                        _rootNavigationService.Navigate(typeof(LoginPage));
                }
                else
                {
                    _navigate = false;
                    _navigateToLoginCommand.RaiseCanExecuteChanged();
                }
            }, ()=>!_navigate));
    }
}

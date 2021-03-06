﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LovelyMother.Uwp.Models;
using LovelyMother.Uwp.Services;

namespace LovelyMother.Uwp.ViewModels
{
    public class UpdateUserViewModel:ViewModelBase
    {
        /// <summary>
        ///     对话框服务。
        /// </summary>
        private readonly IDialogService _dialogService;

        /// <summary>
        ///     身份服务。
        /// </summary>
        private readonly IIdentityService _identityService;

        /// <summary>
        /// 用户服务。
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        ///     根导航服务。
        /// </summary>
        private readonly IRootNavigationService _rootNavigationService;


        private AppUser _currentUser;
        public AppUser CurrentUser
        {
            get => _currentUser;
            set => Set(nameof(CurrentUser), ref _currentUser, value);
        }

        /// <summary>
        ///     构造函数。
        /// </summary>
        /// <param name="identityService">身份服务。</param>
        /// <param name="rootNavigationService">根导航服务。</param>
        /// <param name="dialogService">对话框服务。</param>
        public UpdateUserViewModel(IIdentityService identityService,
            IRootNavigationService rootNavigationService,
            IDialogService dialogService,
            IUserService userService)
        {
            _identityService = identityService;
            _rootNavigationService = rootNavigationService;
            _dialogService = dialogService;
            _userService = userService;
            CurrentUser = new AppUser();

            CurrentUser.ID = _identityService.GetCurrentUserAsync().ID;
            CurrentUser.UserName = _identityService.GetCurrentUserAsync().UserName;
            CurrentUser.TotalTime = _identityService.GetCurrentUserAsync().TotalTime;
            CurrentUser.ApplicationUserID = _identityService.GetCurrentUserAsync().ApplicationUserID;
            CurrentUser.Image = _identityService.GetCurrentUserAsync().Image;


        }


        public void refresh()
        {


                CurrentUser.ID = _identityService.GetCurrentUserAsync().ID;
                CurrentUser.UserName = _identityService.GetCurrentUserAsync().UserName;
                CurrentUser.TotalTime = _identityService.GetCurrentUserAsync().TotalTime;
                CurrentUser.ApplicationUserID = _identityService.GetCurrentUserAsync().ApplicationUserID;
                CurrentUser.Image = _identityService.GetCurrentUserAsync().Image;

        }


        /// <summary>
        ///     刷新命令。
        /// </summary>
        private RelayCommand _refreshCommand;

        public RelayCommand RefreshCommand =>
            _refreshCommand ?? (_refreshCommand = new RelayCommand( () => {
            
                refresh();
            }));

        /// <summary>
        ///     更新用户命令。
        /// </summary>
        private RelayCommand _updateUserCommand;

        public RelayCommand UpdateUserCommand =>
            _updateUserCommand ?? (_updateUserCommand = new RelayCommand(async () => {

                var thisuser = _identityService.GetCurrentUserAsync();
                if (thisuser.ApplicationUserID != null)
                {

                    _identityService.SetCurrentUserAsync(CurrentUser.UserName, CurrentUser.TotalTime, CurrentUser.Image);
                    await _userService.UpdateMeAsync(CurrentUser.UserName, CurrentUser.TotalTime, CurrentUser.WeekTotalTime,
                        CurrentUser.Image);
                   refresh();
                }

               

            }));




    }
}

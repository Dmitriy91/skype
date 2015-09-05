﻿using Client;
using Skype.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Skype.View
{
    /// <summary>
    /// Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class AuthenticationWindow : Window
    {
        private readonly AuthenticationWinViewModel _authenticationViewModel;
        
        public AuthenticationWindow()
        {
            InitializeComponent();
            _authenticationViewModel = new AuthenticationWinViewModel((AuthenticationDialogResult dialogResult) => 
            {
                if (this.Dispatcher.CheckAccess())
                {
                    this.Tag = dialogResult;
                    this.Close();
                }
                else
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(()=>
                    {
                        this.Tag = dialogResult;
                        this.Close();
                    }));
                }
            });
            DataContext = _authenticationViewModel;
        }
    }
}

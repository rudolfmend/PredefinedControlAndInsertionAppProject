﻿#pragma checksum "..\..\..\ApplicationSelectorDialog.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "C0DA05E9B9D0256CB21193A6E5875227803411E7"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PredefinedControlAndInsertionAppProject {
    
    
    /// <summary>
    /// ApplicationSelectorDialog
    /// </summary>
    public partial class ApplicationSelectorDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 25 "..\..\..\ApplicationSelectorDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvApplications;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\ApplicationSelectorDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvWindows;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\ApplicationSelectorDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnShowElements;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\ApplicationSelectorDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnRefresh;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\..\ApplicationSelectorDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnSelect;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\..\ApplicationSelectorDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnCancel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.3.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PredefinedControlAndInsertionAppProject;component/applicationselectordialog.xaml" +
                    "", System.UriKind.Relative);
            
            #line 1 "..\..\..\ApplicationSelectorDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.3.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.lvApplications = ((System.Windows.Controls.ListView)(target));
            
            #line 25 "..\..\..\ApplicationSelectorDialog.xaml"
            this.lvApplications.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lvApplications_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.lvWindows = ((System.Windows.Controls.ListView)(target));
            
            #line 46 "..\..\..\ApplicationSelectorDialog.xaml"
            this.lvWindows.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.lvWindows_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 47 "..\..\..\ApplicationSelectorDialog.xaml"
            this.lvWindows.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.lvWindows_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.BtnShowElements = ((System.Windows.Controls.Button)(target));
            
            #line 56 "..\..\..\ApplicationSelectorDialog.xaml"
            this.BtnShowElements.Click += new System.Windows.RoutedEventHandler(this.BtnShowElements_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BtnRefresh = ((System.Windows.Controls.Button)(target));
            
            #line 63 "..\..\..\ApplicationSelectorDialog.xaml"
            this.BtnRefresh.Click += new System.Windows.RoutedEventHandler(this.BtnRefresh_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.BtnSelect = ((System.Windows.Controls.Button)(target));
            
            #line 65 "..\..\..\ApplicationSelectorDialog.xaml"
            this.BtnSelect.Click += new System.Windows.RoutedEventHandler(this.BtnSelect_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.BtnCancel = ((System.Windows.Controls.Button)(target));
            
            #line 67 "..\..\..\ApplicationSelectorDialog.xaml"
            this.BtnCancel.Click += new System.Windows.RoutedEventHandler(this.BtnCancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


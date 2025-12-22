// Copyright (c) Georg Jung. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace BlazorFace.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage());
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
        {
            window.Title = "Understanding Face Recognition";
        }

        return window;
    }
}

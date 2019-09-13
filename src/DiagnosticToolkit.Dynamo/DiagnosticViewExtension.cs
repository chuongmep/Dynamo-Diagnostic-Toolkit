﻿using DiagnosticToolkit.UI.ViewModels;
using DiagnosticToolkit.UI.Views;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DiagnosticToolkit.Dynamo
{
    public class DiagnosticViewExtension : IViewExtension
    {
        public const string NAME = "Diagnostic Toolkit";

        private readonly Version MINIMUM_VERSION = new Version("2.3.0.5885");
        private Version currentVersion { get; set; }
        private MenuItem mainMenu { get; set; }
        private ViewLoadedParams loadedParameters { get; set; }

        public string UniqueId => "EF1D3B3E-3991-46BE-B75F-8429A49AE58C";
        public string Name => NAME;

        private DynamoProfilingManager manager { get; set; }

        public void Startup(ViewStartupParams parameters)
        {
            this.currentVersion = parameters.DynamoVersion;
        }

        public void Loaded(ViewLoadedParams parameters)
        {
            this.loadedParameters = parameters;

            bool canRunExtension = this.currentVersion >= MINIMUM_VERSION;
            if (canRunExtension)
                this.manager = new DynamoProfilingManager(this.loadedParameters);

            this.InitializeMenu(canRunExtension);
        }

        public void InitializeMenu(bool canRunExtension)
        {
            this.mainMenu = new MenuItem() { Header = NAME };

            if (!canRunExtension)
            {
                this.mainMenu.Click += (sender, arg) =>
                {
                    MessageBox.Show(
                        $"Diagnostic Toolkit cannot be used on your current Dynamo Version {this.currentVersion}.\nMinimum version required is {MINIMUM_VERSION}",
                        this.Name);
                };
                return;
            }

            var launchToolkit = new MenuItem() { Header = "Launch" };

            launchToolkit.Click += (sender, args) =>
            {
                DiagnosticMainViewModel viewModel = new DiagnosticMainViewModel(this.manager);
                DiagnosticMainView view = new DiagnosticMainView(viewModel);
                view.Show();
            };

            var enableCheckbox = new CheckBox() {
                Content = "Profiling Enabled",
                IsChecked = this.manager.IsEnabled
            };
            enableCheckbox.Click += (sender, arg) =>
            {
                if (sender is CheckBox check && check.IsChecked.Value)
                    this.manager.EnableProfiling();
                else
                    this.manager.DisableProfiling();
            };

            this.mainMenu.Items.Add(launchToolkit);
            this.mainMenu.Items.Add(enableCheckbox);
            this.loadedParameters.dynamoMenu.Items.Add(this.mainMenu);
        }

        public void Shutdown()
        {
            this.manager?.Dispose();
        }

        public void Dispose()
        {
            manager.Dispose();
        }
    }
}
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using EvolutionOptimization.Interfaces;
using EvolutionOptimization.Models;
using GalaSoft.MvvmLight.CommandWpf;
using Viewer3D.Annotations;

namespace Viewer3D.Controls.Configuration.ViewModels
{
    public class ConfigurationViewModel : IConfiguration, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public EvolutionOptimization.Models.Configuration Config;
        private static double Tolerance = 0.01;

        #region Commands
        public RelayCommand<TextCompositionEventArgs> PreviewTextInputForIntegersCommand { get; set; }
        public RelayCommand<TextCompositionEventArgs> PreviewTextInputForDoublesCommand { get; set; }
        #endregion

        #region Properties
        public bool HasDoubleValues
        {
            get => Config.HasDoubleValues; set
            {
                if (Config.HasDoubleValues == value) return;
                Config.HasDoubleValues = value;
                OnPropertyChanged();
            }
        }

        public int Digits
        {
            get => Config.Digits; set
            {
                if (Config.Digits == value) return;
                Config.Digits = value;
                OnPropertyChanged();
            }
        }

        public int MaxNumberOfSteps
        {
            get => Config.MaxNumberOfSteps; set
            {
                if (Config.MaxNumberOfSteps == value) return;
                Config.MaxNumberOfSteps = value;
                OnPropertyChanged();
            }
        }

        public int NumberOfGenes
        {
            get => Config.NumberOfGenes; set
            {
                if (Config.NumberOfGenes == value) return;
                Config.NumberOfGenes = value;
                OnPropertyChanged();
            }
        }

        public int PopSize
        {
            get => Config.PopSize; set
            {
                if (Config.PopSize == value) return;
                Config.PopSize = value;
                OnPropertyChanged();
            }
        }

        public double MutateChange
        {
            get => Config.MutateChange; set
            {
                if (Math.Abs(Config.MutateChange - value) < Tolerance) return;
                Config.MutateChange = value;
                OnPropertyChanged();
            }
        }

        public int MaxGeneration
        {
            get => Config.MaxGeneration; set
            {
                if (Config.MaxGeneration == value) return;
                Config.MaxGeneration = value;
                OnPropertyChanged();
            }
        }

        public double MinX
        {
            get => Config.MinX; set
            {
                if (Math.Abs(Config.MinX - value) < Tolerance) return;
                Config.MinX = value;
                OnPropertyChanged();
            }
        }

        public double MaxX
        {
            get => Config.MaxX; set
            {
                if (Math.Abs(Config.MaxX - value) < Tolerance) return;
                Config.MaxX = value;
                OnPropertyChanged();
            }
        }

        public double MutateRate
        {
            get => Config.MutateRate; set
            {
                if (Math.Abs(Config.MutateRate - value) < Tolerance) return;
                Config.MutateRate = value;
                OnPropertyChanged();
            }
        }

        public double Tau
        {
            get => Config.Tau; set
            {
                if (Math.Abs(Config.Tau - value) < Tolerance) return;
                Config.Tau = value;
                OnPropertyChanged();
            }
        }

        public double ExitError
        {
            get => Config.ExitError; set
            {
                if (Math.Abs(Config.ExitError - value) < Tolerance) return;
                Config.ExitError = value;
                OnPropertyChanged();
            }
        }

        public int RefreshInterval
        {
            get => Config.RefreshInterval; set
            {
                if (Math.Abs(Config.RefreshInterval - value) < Tolerance) return;
                Config.RefreshInterval = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor
        public ConfigurationViewModel()
        {
            PreviewTextInputForIntegersCommand = new RelayCommand<TextCompositionEventArgs>(IntegerNumberValidationTextBox);
            PreviewTextInputForDoublesCommand = new RelayCommand<TextCompositionEventArgs>(DoubleNumberValidationTextBox);
        }

        private void IntegerNumberValidationTextBox(TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DoubleNumberValidationTextBox(TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text) && double.TryParse(e.Text, out _);
        }

        #endregion
    }
}

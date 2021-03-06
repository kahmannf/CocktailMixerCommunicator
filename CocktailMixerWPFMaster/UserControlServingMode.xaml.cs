﻿using CocktailMixerCommunicator.Communication;
using CocktailMixerCommunicator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CocktailMixerWPFMaster
{
    /// <summary>
    /// Interaktionslogik für UserControlServingMode.xaml
    /// </summary>
    public partial class UserControlServingMode : UserControl
    {
        public MainViewModel VmMain { get; set; }


        public UserControlServingMode()
        {
            InitializeComponent();
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ServingModeViewModel).OrderDummyTest();
        }

        private void ButtonRecipes_Click(object sender, RoutedEventArgs e)
        {

            (this.DataContext as ServingModeViewModel).ShowRecipes();
        }

        private void ButtonBeverages_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ServingModeViewModel).ShowBeverages();
        }

        private void ButtonBackToSelection_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as ServingModeViewModel).BackToSelection();
        }
    }
}

using Microsoft.Maui.Controls;
using System;

namespace SortingVisualizer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            AlgorithmPicker.Items.Add("Bubble Sort");
            AlgorithmPicker.Items.Add("Selection Sort");
            AlgorithmPicker.Items.Add("Merge Sort");
            AlgorithmPicker.Items.Add("Quick Sort");
            AlgorithmPicker.Items.Add("Insertion Sort");

        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            if (AlgorithmPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please select a sorting algorithm", "OK");
                return;
            }

            string? selectedAlgorithm = AlgorithmPicker.SelectedItem?.ToString();
            if (selectedAlgorithm != null)
            {
                await Navigation.PushAsync(new VisualizationPage(selectedAlgorithm));
            }
        }
    }
}

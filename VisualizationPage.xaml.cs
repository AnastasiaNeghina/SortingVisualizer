using Microsoft.Maui.Controls;
using System;
using SortingVisualizer.SortingAlgorithms;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SortingVisualizer
{
    public partial class VisualizationPage : ContentPage
    {
        private string _selectedAlgorithm;
        private CancellationTokenSource _cancellationTokenSource;
        private int[] _data;

        public VisualizationPage(string selectedAlgorithm)
        {
            InitializeComponent();
            _selectedAlgorithm = selectedAlgorithm;
            AlgorithmLabel.Text = selectedAlgorithm;
            DescriptionLabel.Text = GetAlgorithmDescription(selectedAlgorithm);
            _cancellationTokenSource = new CancellationTokenSource();
            OnGenerateArrayClicked(null, null); // Generate a random array initially
        }

        private string GetAlgorithmDescription(string algorithm)
        {
            return algorithm switch
            {
                "Bubble Sort" => "Bubble Sort is a simple sorting algorithm that repeatedly steps through the list, compares adjacent elements, and swaps them if they are in the wrong order.",
                "Selection Sort" => "Selection Sort is an in-place comparison sorting algorithm. It works by dividing the list into a sorted and an unsorted part, then selecting the smallest element from the unsorted part.",
                _ => "",
            };
        }

        private void OnGenerateArrayClicked(object sender, EventArgs e)
        {
            Random random = new Random();
            _data = Enumerable.Range(1, 10).Select(x => random.Next(1, 100)).ToArray();
            InitializeVisualizationGrid();
        }

        private void InitializeVisualizationGrid()
        {
            VisualizationGrid.RowDefinitions.Clear();
            VisualizationGrid.ColumnDefinitions.Clear();
            VisualizationGrid.Children.Clear();

            for (int i = 0; i < _data.Length; i++)
            {
                VisualizationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }
            VisualizationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            UpdateVisualizationGrid(_data);
        }

           private async void OnStartCancelClicked(object sender, EventArgs e)
        {
            if (StartCancelButton.Text == "Execute Sort")
            {
                StartCancelButton.Text = "Cancel Visualization";
                _cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    switch (_selectedAlgorithm)
                    {
                        case "Bubble Sort":
                            await BubbleSortVisual(_data, _cancellationTokenSource.Token);
                            break;
                        case "Selection Sort":
                            await SelectionSortVisual(_data, _cancellationTokenSource.Token);
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    await DisplayAlert("Cancelled", "Visualization has been cancelled.", "OK");
                }
                finally
                {
                    StartCancelButton.Text = "Execute Sort";
                }
            }
            else
            {
                _cancellationTokenSource.Cancel();
            }
        }

          private async Task BubbleSortVisual(int[] array, CancellationToken cancellationToken)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - i - 1; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Highlight the elements being compared
                    UpdateVisualizationGrid(array, j, j + 1);
                    await Task.Delay(500, cancellationToken); // Delay for visualization

                    if (array[j] > array[j + 1])
                    {
                        // Swap elements
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;

                        // Update visualization after swap
                        await SmoothSwap(j, j + 1);
                        UpdateVisualizationGrid(array, j, j + 1);
                        await Task.Delay(500, cancellationToken); // Delay for visualization
                    }
                }

                // Highlight the sorted element
                UpdateVisualizationGrid(array, -1, array.Length - i - 1);
            }

            // Highlight all elements as sorted
            UpdateVisualizationGrid(array);
        }

        private async Task SelectionSortVisual(int[] array, CancellationToken cancellationToken)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < array.Length; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Highlight the elements being compared
                    UpdateVisualizationGrid(array, i, j);
                    await Task.Delay(500, cancellationToken); // Delay for visualization

                    if (array[j] < array[minIndex])
                    {
                        minIndex = j;
                    }
                }

                if (minIndex != i)
                {
                    // Swap elements
                    int temp = array[i];
                    array[i] = array[minIndex];
                    array[minIndex] = temp;

                    // Update visualization after swap
                    await SmoothSwap(i, minIndex);
                    UpdateVisualizationGrid(array, i, minIndex);
                    await Task.Delay(500, cancellationToken); // Delay for visualization
                }

                // Highlight the sorted element
                UpdateVisualizationGrid(array, -1, i);
            }

            // Highlight all elements as sorted
            UpdateVisualizationGrid(array);
        }

        private async Task SmoothSwap(int index1, int index2)
        {
            var element1 = VisualizationGrid.Children[index1] as StackLayout;
            var element2 = VisualizationGrid.Children[index2] as StackLayout;

            if (element1 != null && element2 != null)
            {
                var element1InitialPosition = element1.TranslationX;
                var element2InitialPosition = element2.TranslationX;

                var moveElement1 = element1.TranslateTo(element2InitialPosition, 0, 1000, Easing.CubicInOut);
                var moveElement2 = element2.TranslateTo(element1InitialPosition, 0, 1000, Easing.CubicInOut);

                await Task.WhenAll(moveElement1, moveElement2);
            }
        }

         private void UpdateVisualizationGrid(int[] data, int activeIndex1 = -1, int activeIndex2 = -1)
        {
            VisualizationGrid.Children.Clear();
            for (int i = 0; i < data.Length; i++)
            {
                var stackLayout = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    VerticalOptions = LayoutOptions.EndAndExpand,
                    HorizontalOptions = LayoutOptions.Center
                };

                var boxView = new BoxView
                {
                    Color = (i == activeIndex1 || i == activeIndex2) ? Colors.Orange : Colors.CornflowerBlue,
                    HeightRequest = data[i] * 3,
                    WidthRequest = 20
                };
                var label = new Label
                {
                    Text = data[i].ToString(),
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Center
                };

                stackLayout.Children.Add(boxView);
                stackLayout.Children.Add(label);
                VisualizationGrid.Children.Add(stackLayout);
                Grid.SetColumn(stackLayout, i);
                Grid.SetRow(stackLayout, 0);
            }
        }

                private void OnResetClicked(object sender, EventArgs e)
        {
            UpdateVisualizationGrid(_data);
        }

        private async void OnBackToMainPageClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
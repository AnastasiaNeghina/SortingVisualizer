using Microsoft.Maui.Controls;
using System;
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
                "Merge Sort" => "Merge Sort is a divide-and-conquer algorithm that splits the list into smaller sublists, sorts them, and then merges them back together.",
                "Quick Sort" => "Quick Sort is an efficient divide-and-conquer algorithm that selects a 'pivot' element and partitions the array into two sub-arrays: elements less than the pivot and elements greater than the pivot. It then recursively sorts the sub-arrays.",
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
                        case "Merge Sort":
                            await MergeSortVisual(_data, 0, _data.Length - 1, _cancellationTokenSource.Token);
                            break;
                        case "Quick Sort":
                            await QuickSortVisual(_data, 0, _data.Length - 1, _cancellationTokenSource.Token);
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

        private async Task MergeSortVisual(int[] array, int left, int right, CancellationToken cancellationToken, int depth = 0)
        {
            if (left < right)
            {
                // Highlight the current sub-array being divided
                UpdateVisualizationGrid(array, left, right, Colors.LightBlue, depth);

                int middle = (left + right) / 2;

                // Wait to show the division
                await Task.Delay(500, cancellationToken);

                // Sort left and right halves recursively
                await MergeSortVisual(array, left, middle, cancellationToken, depth + 1);
                await MergeSortVisual(array, middle + 1, right, cancellationToken, depth + 1);

                // Highlight before merging
                UpdateVisualizationGrid(array, left, right, Colors.LightGreen, depth);
                await Task.Delay(500, cancellationToken); // Wait to show merging process

                // Merge and show the merging process
                await Merge(array, left, middle, right, cancellationToken);
            }
        }

        private async Task Merge(int[] array, int left, int middle, int right, CancellationToken cancellationToken)
        {
            int n1 = middle - left + 1;
            int n2 = right - middle;

            int[] leftArray = new int[n1];
            int[] rightArray = new int[n2];

            for (int i = 0; i < n1; ++i)
                leftArray[i] = array[left + i];
            for (int j = 0; j < n2; ++j)
                rightArray[j] = array[middle + 1 + j];

            int iLeft = 0, iRight = 0;
            int k = left;
            while (iLeft < n1 && iRight < n2)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Highlight the current elements being compared
                UpdateVisualizationGrid(array, k, highlightColor: Colors.Orange);
                await Task.Delay(500, cancellationToken); // Delay to show comparison

                if (leftArray[iLeft] <= rightArray[iRight])
                {
                    array[k] = leftArray[iLeft];
                    iLeft++;
                }
                else
                {
                    array[k] = rightArray[iRight];
                    iRight++;
                }

                // Update visualization after placing an element
                UpdateVisualizationGrid(array, k);
                await Task.Delay(500, cancellationToken);
                k++;
            }

            // Copy remaining elements from leftArray, if any
            while (iLeft < n1)
            {
                cancellationToken.ThrowIfCancellationRequested();
                array[k] = leftArray[iLeft];

                // Update visualization after placing an element
                UpdateVisualizationGrid(array, k, highlightColor: Colors.LightGreen);
                await Task.Delay(500, cancellationToken);

                iLeft++;
                k++;
            }

            // Copy remaining elements from rightArray, if any
            while (iRight < n2)
            {
                cancellationToken.ThrowIfCancellationRequested();
                array[k] = rightArray[iRight];

                // Update visualization after placing an element
                UpdateVisualizationGrid(array, k, highlightColor: Colors.LightGreen);
                await Task.Delay(500, cancellationToken);

                iRight++;
                k++;
            }
        }

        public async Task QuickSortVisual(int[] array, int left, int right, CancellationToken cancellationToken)
        {
            if (left < right)
            {
                // Highlight the current sub-array being partitioned
                UpdateVisualizationGrid(array, left, right, Colors.LightBlue);
                await Task.Delay(500, cancellationToken);

                // Partition the array and get the pivot index
                int pivotIndex = await Partition(array, left, right, cancellationToken);

                // Highlight pivot element
                UpdateVisualizationGrid(array, pivotIndex, pivotIndex, Colors.Red);
                await Task.Delay(500, cancellationToken);

                // Recursively sort elements before and after partition
                await QuickSortVisual(array, left, pivotIndex - 1, cancellationToken);
                await QuickSortVisual(array, pivotIndex + 1, right, cancellationToken);
            }
        }

        private async Task<int> Partition(int[] array, int left, int right, CancellationToken cancellationToken)
        {
            int pivot = array[right];
            int i = left - 1;

            // Highlight pivot element initially
            UpdateVisualizationGrid(array, right, right, Colors.Red);
            await Task.Delay(500, cancellationToken);

            for (int j = left; j < right; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Highlight elements being compared
                UpdateVisualizationGrid(array, i + 1, j, Colors.Orange);
                await Task.Delay(500, cancellationToken);

                if (array[j] <= pivot)
                {
                    i++;

                    // Swap array[i] and array[j]
                    int temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                }
            }
            // Swap array[i + 1] and array[right] (placing pivot in correct location)
            int tempPivot = array[i + 1];
            array[i + 1] = array[right];
            array[right] = tempPivot;

            // Highlight pivot placement
            UpdateVisualizationGrid(array, i + 1, right, Colors.CornflowerBlue);
            await Task.Delay(500, cancellationToken);

            return i + 1;
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

        private void UpdateVisualizationGrid(int[] data, int left = -1, int right = -1, Color highlightColor = default, int depth = 0)
        {
            VisualizationGrid.Children.Clear();

            // Set a default maxHeight in case VisualizationGrid.Height is not properly initialized
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300; // Default to 300 if not set
            int maxDataValue = data.Max(); // Find the maximum value in the data array

            for (int i = 0; i < data.Length; i++)
            {
                var stackLayout = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    VerticalOptions = LayoutOptions.EndAndExpand,
                    HorizontalOptions = LayoutOptions.Center
                };

                // Determine the color based on the range being visualized
                Color barColor;
                if (left != -1 && right != -1 && i >= left && i <= right)
                {
                    barColor = highlightColor == default ? Colors.LightBlue : highlightColor;
                }
                else
                {
                    barColor = Colors.CornflowerBlue; // Default color for bars
                }

                // Calculate relative height to fit within the container
                double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0; // 90% of maxHeight for padding

                var boxView = new BoxView
                {
                    Color = barColor,
                    HeightRequest = relativeHeight,
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
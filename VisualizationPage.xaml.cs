using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
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

            // Initialize class variables
            _selectedAlgorithm = selectedAlgorithm;
            _cancellationTokenSource = new CancellationTokenSource();

            // Set up the algorithm title and description
            AlgorithmLabel.Text = selectedAlgorithm;
            DescriptionLabel.Text = GetAlgorithmDescription(selectedAlgorithm);

            // Generate a random array initially
            OnGenerateArrayClicked(null, null);

            // Dynamically update the legend based on the algorithm
            UpdateLegend(_selectedAlgorithm);
        }

        private string GetAlgorithmDescription(string algorithm)
        {
            return algorithm switch
            {
                "Bubble Sort" => "Bubble Sort is a simple sorting algorithm that repeatedly steps through the list, compares adjacent elements, and swaps them if they are in the wrong order.",
                "Selection Sort" => "Selection Sort is an in-place comparison sorting algorithm. It works by dividing the list into a sorted and an unsorted part, then selecting the smallest element from the unsorted part.",
                "Merge Sort" => "Merge Sort is a divide-and-conquer algorithm that splits the list into smaller sublists, sorts them, and then merges them back together.",
                "Quick Sort" => "Quick Sort is an efficient divide-and-conquer algorithm that selects a 'pivot' element and partitions the array into two sub-arrays: elements less than the pivot and elements greater than the pivot. It then recursively sorts the sub-arrays.",
                "Insertion Sort" => "Insertion Sort is a simple, in-place comparison-based sorting algorithm that builds the final sorted array one item at a time by repeatedly inserting elements into their correct position within the sorted part of the array.",
                _ => "",
            };
        }

        private void OnGenerateArrayClicked(object sender, EventArgs e)
{
    Random random = new Random();
    
    // Generate a range of unique numbers (1 to 100) and shuffle them
    _data = Enumerable.Range(1, 99) // Create a range of numbers from 1 to 100
                      .OrderBy(x => random.Next()) // Shuffle the numbers randomly
                      .Take(20) // Take the first 20 numbers
                      .ToArray(); // Convert to array

    // Initialize the visualization grid with the unique array
    InitializeVisualizationGrid();
}


        private void InitializeVisualizationGrid()
        {
            VisualizationGrid.RowDefinitions.Clear();
            VisualizationGrid.ColumnDefinitions.Clear();
            VisualizationGrid.Children.Clear();

            // Set fixed dimensions for VisualizationGrid
            VisualizationGrid.HeightRequest = 300; // Set a fixed height
            VisualizationGrid.WidthRequest = 400; // Set a fixed width (optional)

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
                        case "Insertion Sort":
                            await InsertionSortVisual(_data, _cancellationTokenSource.Token);
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

        // Bubble Sort
        private bool[] _finalPositionMarkers; // Field to track final positions
        private int _lastFinalPositionIndex; // Track the most recent final position

        private async Task BubbleSortVisual(int[] array, CancellationToken cancellationToken)
        {
            int n = array.Length;

            // Initialize the final position markers
            _finalPositionMarkers = new bool[n];
            _lastFinalPositionIndex = -1;

            for (int i = 0; i < n - 1; i++)
            {
                bool swapped = false;

                for (int j = 0; j < n - i - 1; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await UpdateVisualizationGridBubble(array, j, j + 1, Colors.Orange);
                    await Task.Delay(1000, cancellationToken);

                    if (array[j] > array[j + 1])
                    {
                        _lastFinalPositionIndex = n - i - 1;

                        await SmoothSwap(j, j + 1);

                        (array[j], array[j + 1]) = (array[j + 1], array[j]);
                        swapped = true;

                        await UpdateVisualizationGridBubble(array, j, j + 1);
                        await Task.Delay(500, cancellationToken);
                    }
                    else
                    {
                        await UpdateVisualizationGridBubble(array, j, j + 1);
                        await Task.Delay(500, cancellationToken);
                    }
                }

                if (!swapped)
                {
                    for (int k = n - i - 1; k < n; k++)
                    {
                        _finalPositionMarkers[k] = true;
                    }
                    break;
                }
                else
                {
                    _finalPositionMarkers[_lastFinalPositionIndex] = true;
                }
            }

            await UpdateVisualizationGridBubble(array);
        }

        private async Task UpdateVisualizationGridBubble(int[] data, int left = -1, int right = -1, Color highlightColor = default)
        {
            double maxHeight = 300;
            int maxDataValue = data.Max();

            for (int i = 0; i < data.Length; i++)
            {
                var stackLayout = VisualizationGrid.Children[i] as StackLayout;
                var boxView = stackLayout.Children[0] as BoxView;
                var label = stackLayout.Children[1] as Label;

                boxView.Color = Colors.CornflowerBlue;

                // Check if the element is in its final sorted position
                if (_finalPositionMarkers != null && _finalPositionMarkers[i])
                {
                    boxView.Color = Colors.BlueViolet;
                }

                // Color the comparison bars in orange
                if (i >= left && i <= right)
                {
                    boxView.Color = Colors.Orange;
                }

                // Update height if needed
                double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;
                boxView.HeightRequest = relativeHeight;
                label.Text = data[i].ToString();
            }

            // Optional: Add a small delay to make color changes visible
            await Task.Delay(200);
        }

        // Selection Sort
        private async Task SelectionSortVisual(int[] array, CancellationToken cancellationToken)
        {
            HashSet<int> sortedIndices = new HashSet<int>();

            for (int i = 0; i < array.Length - 1; i++)
            {
                int minIndex = i;

                // Highlight the current minimum element (in red)
                await UpdateVisualizationGridSelection(array, currentMinIndex: minIndex, sortedUntil: sortedIndices);
                await Task.Delay(500, cancellationToken);

                for (int j = i + 1; j < array.Length; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Highlight the element being compared to the current minimum (in LightCoral)
                    await UpdateVisualizationGridSelection(array, currentMinIndex: minIndex, comparedIndex: j, sortedUntil: sortedIndices);
                    await Task.Delay(500, cancellationToken);

                    if (array[j] < array[minIndex])
                    {
                        minIndex = j;

                        await UpdateVisualizationGridSelection(array, currentMinIndex: minIndex, sortedUntil: sortedIndices);
                        await Task.Delay(500, cancellationToken);
                    }
                }

                if (minIndex != i)
                {
                    await SmoothSwap(i, minIndex);

                    int temp = array[i];
                    array[i] = array[minIndex];
                    array[minIndex] = temp;
                }

                sortedIndices.Add(i);

                await UpdateVisualizationGridSelection(array, sortedUntil: sortedIndices);
                await Task.Delay(500, cancellationToken);
            }

            sortedIndices.Add(array.Length - 1);
            await UpdateVisualizationGridSelection(array, sortedUntil: sortedIndices);
        }

        private async Task UpdateVisualizationGridSelection(int[] data, int currentMinIndex = -1, int comparedIndex = -1, HashSet<int> sortedUntil = null)
        {
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300;
            int maxDataValue = data.Max();
            sortedUntil ??= new HashSet<int>();

            for (int i = 0; i < data.Length; i++)
            {
                var stackLayout = VisualizationGrid.Children[i] as StackLayout;
                if (stackLayout == null) continue;

                var boxView = stackLayout.Children[0] as BoxView;
                if (boxView == null) continue;

                var label = stackLayout.Children[1] as Label;
                if (label == null) continue;

                double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;
                boxView.HeightRequest = relativeHeight;
                label.Text = data[i].ToString();

                if (sortedUntil.Contains(i))
                {
                    boxView.Color = Colors.BlueViolet;
                }
                else if (i == currentMinIndex)
                {
                    boxView.Color = Colors.Red;
                }
                else if (i == comparedIndex)
                {
                    boxView.Color = Colors.Orange;
                }
                else
                {
                    boxView.Color = Colors.CornflowerBlue;
                }
            }

            await Task.CompletedTask;
        }

        // Merge Sort
        private int _initialMaxValue;

        private async Task MergeSortVisual(int[] array, int left, int right, CancellationToken cancellationToken, int depth = 0)
        {
            if (left == 0 && right == array.Length - 1) // Initialize the max value at the start of sorting
            {
                _initialMaxValue = array.Max();
            }

            if (left < right)
            {
                await HighlightSubArray(array, left, right);
                await Task.Delay(500, cancellationToken);

                int middle = (left + right) / 2;

                await MergeSortVisual(array, left, middle, cancellationToken, depth + 1);
                await MergeSortVisual(array, middle + 1, right, cancellationToken, depth + 1);

                await Merge(array, left, middle, right, cancellationToken);
            }
        }

        private async Task Merge(int[] array, int left, int middle, int right, CancellationToken cancellationToken)
        {
            int n1 = middle - left + 1;
            int n2 = right - middle;

            // Create temporary arrays
            int[] leftArray = new int[n1];
            int[] rightArray = new int[n2];

            // Copy data to temporary arrays
            Array.Copy(array, left, leftArray, 0, n1);
            Array.Copy(array, middle + 1, rightArray, 0, n2);

            int iLeft = 0, iRight = 0;
            int k = left;

            // SmoothSwap Merge Implementation
            while (iLeft < n1 && iRight < n2)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (leftArray[iLeft] <= rightArray[iRight])
                {
                    // If the current element is already in the correct position, skip unnecessary swap
                    if (array[k] != leftArray[iLeft])
                    {
                        array[k] = leftArray[iLeft];
                        await UpdateVisualizationGridMergeImproved(array, left, right);
                        await Task.Delay(500, cancellationToken);
                    }
                    iLeft++;
                }
                else
                {
                    // If the current element is already in the correct position, skip unnecessary swap
                    if (array[k] != rightArray[iRight])
                    {
                        array[k] = rightArray[iRight];
                        await UpdateVisualizationGridMergeImproved(array, left, right);
                        await Task.Delay(500, cancellationToken);
                    }
                    iRight++;
                }

                k++;
            }

            // Handle remaining elements in leftArray
            while (iLeft < n1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // SmoothSwap: Only update if the value is different
                if (array[k] != leftArray[iLeft])
                {
                    array[k] = leftArray[iLeft];
                    await UpdateVisualizationGridMergeImproved(array, left, right);
                    await Task.Delay(500, cancellationToken);
                }

                iLeft++;
                k++;
            }

            // Handle remaining elements in rightArray
            while (iRight < n2)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // SmoothSwap: Only update if the value is different
                if (array[k] != rightArray[iRight])
                {
                    array[k] = rightArray[iRight];
                    await UpdateVisualizationGridMergeImproved(array, left, right);
                    await Task.Delay(500, cancellationToken);
                }

                iRight++;
                k++;
            }
        }

        private async Task UpdateVisualizationGridMergeImproved(int[] data, int left, int right)
        {
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300;

            for (int i = 0; i < data.Length; i++)
            {
                var existingStackLayout = VisualizationGrid.Children
                    .OfType<StackLayout>()
                    .FirstOrDefault(sl => Grid.GetColumn(sl) == i);

                if (existingStackLayout != null)
                {
                    var boxView = existingStackLayout.Children.OfType<BoxView>().FirstOrDefault();
                    var label = existingStackLayout.Children.OfType<Label>().FirstOrDefault();

                    if (boxView != null && label != null)
                    {
                        // Use the initial maximum value for height calculation
                        double relativeHeight = _initialMaxValue > 0 ? (data[i] / (double)_initialMaxValue) * (maxHeight * 0.9) : 0;

                        boxView.HeightRequest = relativeHeight;
                        label.Text = data[i].ToString();

                        // Color logic for current sub-array
                        if (i >= left && i <= right)
                        {
                            boxView.Color = Colors.BlueViolet;
                            label.TextColor = Colors.Black;
                        }
                        else
                        {
                            boxView.Color = Colors.DarkGray;
                            label.TextColor = Colors.DarkGray;
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }

        // Quick Sort
        private HashSet<int> sortedIndices = new HashSet<int>();

        public async Task QuickSortVisual(int[] array, int left, int right, CancellationToken cancellationToken)
        {
            if (left == 0 && right == array.Length - 1) // Clear sorted indices at the beginning of sorting
            {
                sortedIndices.Clear();
            }

            if (left < right)
            {
                int pivotIndex = await Partition(array, left, right, cancellationToken);

                // Mark pivot as sorted and update visualization
                sortedIndices.Add(pivotIndex);
                await UpdateVisualizationGridQuick(array, pivotIndex, pivotIndex, Colors.BlueViolet);
                await Task.Delay(500, cancellationToken);

                // Recursively sort the sub-arrays
                await QuickSortVisual(array, left, pivotIndex - 1, cancellationToken);
                await QuickSortVisual(array, pivotIndex + 1, right, cancellationToken);
            }
            else if (left == right) // Handle the case where a single element is sorted
            {
                sortedIndices.Add(left);
                await UpdateVisualizationGridQuick(array, left, left, Colors.BlueViolet);
            }
        }

        private async Task<int> Partition(int[] array, int left, int right, CancellationToken cancellationToken)
        {
            int pivot = array[right];
            int i = left - 1;

            // Highlight pivot element once at the start
            await UpdateVisualizationGridQuick(array, right, right, Colors.Red);
            await Task.Delay(500, cancellationToken);

            for (int j = left; j < right; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Highlight current comparison with a different color
                await UpdateVisualizationGridQuick(array, right, j, Colors.Red, Colors.Orange);
                await Task.Delay(500, cancellationToken);

                if (array[j] <= pivot)
                {
                    i++;
                    await SmoothSwap(i, j);

                    // Perform actual swap in the array
                    (array[i], array[j]) = (array[j], array[i]);

                    // Update visualization after swap
                    await UpdateVisualizationGridQuick(array, right, i, Colors.Red);
                    await Task.Delay(500, cancellationToken);
                }
            }

            // Swap array[i + 1] and array[right] to put pivot in correct position
            await SmoothSwap(i + 1, right);
            (array[i + 1], array[right]) = (array[right], array[i + 1]);

            // Mark the pivot element as sorted and update visualization
            sortedIndices.Add(i + 1);
            await UpdateVisualizationGridQuick(array, i + 1, i + 1, Colors.BlueViolet);
            await Task.Delay(500, cancellationToken);

            return i + 1;
        }

        private async Task UpdateVisualizationGridQuick(int[] data, int pivotIndex = -1, int compareIndex = -1, Color pivotColor = default, Color compareColor = default)
        {
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300;
            int maxDataValue = data.Max();
            int pivot = pivotIndex >= 0 ? data[pivotIndex] : -1;

            for (int i = 0; i < data.Length; i++)
            {
                var stackLayout = VisualizationGrid.Children[i] as StackLayout;
                if (stackLayout == null) continue;

                var boxView = stackLayout.Children[0] as BoxView;
                if (boxView == null) continue;

                var label = stackLayout.Children[1] as Label;
                if (label == null) continue;

                // Update height and label text
                double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;
                boxView.HeightRequest = relativeHeight;
                label.Text = data[i].ToString();

                // Set color based on sorted indices, pivot, or current highlighting
                if (sortedIndices.Contains(i))
                {
                    boxView.Color = Colors.BlueViolet;
                }
                else if (pivot != -1)
                {
                    if (data[i] < pivot)
                    {
                        boxView.Color = Colors.Green; // Elements less than pivot
                    }
                    else if (data[i] > pivot)
                    {
                        boxView.Color = Colors.DarkBlue; // Elements greater than pivot
                    }
                    else // data[i] == pivot
                    {
                        boxView.Color = Colors.Yellow; // Elements equal to pivot
                    }
                }

                // Pivot and comparison highlighting
                if (i == pivotIndex)
                {
                    boxView.Color = pivotColor == default ? Colors.Red : pivotColor;
                }
                else if (i == compareIndex)
                {
                    boxView.Color = compareColor == default ? Colors.Orange : compareColor;
                }

                // Default color if no other condition is met
                if (boxView.Color == default)
                {
                    boxView.Color = Colors.CornflowerBlue;
                }
            }

            await Task.CompletedTask;
        }

        // Insertion Sort
private Dictionary<int, bool> _sortedElements;

public async Task InsertionSortVisual(int[] array, CancellationToken cancellationToken)
{
    // Initialize the dictionary to track sorted status
    _sortedElements = new Dictionary<int, bool>();

    foreach (var value in array)
    {
        if (!_sortedElements.ContainsKey(value))
        {
            _sortedElements[value] = false;
        }
    }

    // Mark the first element as sorted
    _sortedElements[array[0]] = true;
    await UpdateVisualizationGridInsertion(array, array[0], -1, Colors.BlueViolet);

    for (int i = 1; i < array.Length; i++)
    {
        int key = array[i];
        int j = i - 1;

        // Highlight the key value in red
        await UpdateVisualizationGridInsertion(array, key, -1, Colors.Red);
        await Task.Delay(500, cancellationToken);

        // Continue while the current element is greater than the key and shift the elements
        while (j >= 0 && array[j] > key)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Highlight the comparison bar in orange
            await UpdateVisualizationGridInsertion(array, key, j, Colors.Orange);
            await Task.Delay(500, cancellationToken);

            // Swap elements visually
            await SmoothSwap(j, j + 1);

            // Perform the actual shift in the array
            (array[j + 1], array[j]) = (array[j], array[j + 1]);

            // Update the visualization - keep key red and reset comparison to default
            await UpdateVisualizationGridInsertion(array, key, -1, Colors.Red);
            await Task.Delay(500, cancellationToken);

            // Decrement `j` to continue comparing and shifting if necessary
            j--;
        }

        // Place the key in the correct position
        array[j + 1] = key;

        // Mark the element as sorted
        _sortedElements[key] = true;

        // Update the visualization with BlueViolet for sorted elements
        await UpdateVisualizationGridInsertion(array, key, -1, Colors.BlueViolet);
        await Task.Delay(500, cancellationToken);
    }

    // Ensure all sorted elements retain BlueViolet at the end
    await UpdateVisualizationGridInsertion(array);
}


private async Task UpdateVisualizationGridInsertion(
    int[] data,
    int keyValue = -1,
    int compareIndex = -1,
    Color highlightColor = default)
{
    double maxHeight = 300; // Fixed maximum height for the bars
    int maxDataValue = data.Max();

    for (int i = 0; i < data.Length; i++)
    {
        var stackLayout = VisualizationGrid.Children[i] as StackLayout;
        var boxView = stackLayout.Children[0] as BoxView;
        var label = stackLayout.Children[1] as Label;

        // Check if the element is sorted
        if (_sortedElements.TryGetValue(data[i], out bool isSorted) && isSorted)
        {
            boxView.Color = Colors.BlueViolet; // Keep sorted elements as BlueViolet
        }
        else
        {
            // Reset to default color
            boxView.Color = Colors.CornflowerBlue;
        }

        // Highlight the current element (key) in red
        if (data[i] == keyValue)
        {
            boxView.Color = Colors.Red;
        }

        // Highlight the comparison bar in orange
        if (i == compareIndex)
        {
            boxView.Color = Colors.Orange;
        }

        // Update the bar's height
        double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;
        boxView.HeightRequest = relativeHeight;

        // Update the label
        label.Text = data[i].ToString();
    }

    // Optional: Add a small delay to make color changes visible
    await Task.Delay(200);
}


        // Smooth Swap Animation
        private async Task SmoothSwap(int index1, int index2)
        {
            if (index1 == index2) return;

            var element1 = VisualizationGrid.Children[index1] as StackLayout;
            var element2 = VisualizationGrid.Children[index2] as StackLayout;

            if (element1 != null && element2 != null)
            {
                double element1InitialPosition = element1.TranslationX;
                double element2InitialPosition = element2.TranslationX;

                var moveElement1 = element1.TranslateTo(element2InitialPosition - element1InitialPosition, 0, 200, Easing.CubicInOut);
                var moveElement2 = element2.TranslateTo(element1InitialPosition - element2InitialPosition, 0, 200, Easing.CubicInOut);

                await Task.WhenAll(moveElement1, moveElement2);
            }
        }

        // Update Visualization Grid
        private async Task UpdateVisualizationGrid(int[] data, int left = -1, int right = -1, Color highlightColor = default, string sortingType = "")
        {
            double maxHeight = 300; // Fixed maximum height for the bars
            int maxDataValue = data.Max();

            // Add elements only if not already added
            if (VisualizationGrid.Children.Count == 0)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var stackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(1)
                    };

                    double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;

                    var boxView = new BoxView
                    {
                        Color = Colors.CornflowerBlue, // Default color
                        HeightRequest = relativeHeight,
                        WidthRequest = 10
                    };

                    var label = new Label
                    {
                        Text = data[i].ToString(),
                        FontSize = 10,
                        HorizontalOptions = LayoutOptions.Center
                    };

                    stackLayout.Children.Add(boxView);
                    stackLayout.Children.Add(label);
                    VisualizationGrid.Children.Add(stackLayout);
                    Grid.SetColumn(stackLayout, i);
                    Grid.SetRow(stackLayout, 0);
                }
            }

            // Update only the necessary elements
            for (int i = 0; i < data.Length; i++)
            {
                var stackLayout = VisualizationGrid.Children[i] as StackLayout;
                if (stackLayout == null) continue;

                var boxView = stackLayout.Children[0] as BoxView;
                if (boxView == null) continue;

                var label = stackLayout.Children[1] as Label;
                if (label == null) continue;

                // Update the height and text
                double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;
                boxView.HeightRequest = relativeHeight;
                label.Text = data[i].ToString();

                // Highlight based on sorting type
                if (left != -1 && i >= left && i <= right)
                {
                    boxView.Color = highlightColor == default ? Colors.Orange : highlightColor;
                }
                else
                {
                    boxView.Color = Colors.CornflowerBlue;
                }
            }

            await Task.CompletedTask;
        }

        private async Task HighlightSubArray(int[] data, int left, int right)
        {
            await UpdateVisualizationGrid(data, left, right, Colors.DarkGrey);
        }

        private void OnResetClicked(object sender, EventArgs e)
        {
            UpdateVisualizationGrid(_data);
        }

        private async void OnBackToMainPageClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private void UpdateLegend(string algorithm)
        {
            // Clear previous legend items from the grid
            LegendGrid.Children.Clear();

            // Reset the grid structure
            LegendGrid.RowDefinitions.Clear();
            LegendGrid.ColumnDefinitions.Clear();
            LegendGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Color column
            LegendGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }); // Text column

            // Add the first row for the legend title
            LegendGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Label titleLabel = new Label
            {
                Text = "Legend:",
                FontAttributes = FontAttributes.Bold,
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Start
            };
            Grid.SetRow(titleLabel, 0);
            Grid.SetColumn(titleLabel, 0);
            Grid.SetColumnSpan(titleLabel, 2); // Span both columns
            LegendGrid.Children.Add(titleLabel);

            // Add legend items based on the algorithm
            int currentRow = 1; // Start from the row after the title
            switch (algorithm)
            {
                case "Bubble Sort":
                    AddLegendItem(LegendGrid, "Comparison Elements", Colors.Orange, currentRow++);
                    AddLegendItem(LegendGrid, "Sorted", Colors.BlueViolet, currentRow++);
                    break;

                case "Selection Sort":
                    AddLegendItem(LegendGrid, "Current Minimum", Colors.Red, currentRow++);
                    AddLegendItem(LegendGrid, "Current Comparison", Colors.Orange, currentRow++);
                    AddLegendItem(LegendGrid, "Sorted", Colors.BlueViolet, currentRow++);
                    break;

                case "Merge Sort":
                    AddLegendItem(LegendGrid, "Active Subarray / Sorted", Colors.BlueViolet, currentRow++);
                    AddLegendItem(LegendGrid, "Inactive Subarray", Colors.Gray, currentRow++);
                    break;

                case "Quick Sort":
                    AddLegendItem(LegendGrid, "Pivot", Colors.Red, currentRow++);
                    AddLegendItem(LegendGrid, "Elements equal to pivot", Colors.Yellow, currentRow++);
                    AddLegendItem(LegendGrid, "Elements less than pivot", Colors.Green, currentRow++);
                    AddLegendItem(LegendGrid, "Elements greater than pivot", Colors.DarkBlue, currentRow++);
                    AddLegendItem(LegendGrid, "Current Comparison", Colors.Orange, currentRow++);
                    AddLegendItem(LegendGrid, "Sorted", Colors.BlueViolet, currentRow++);
                    break;

                case "Insertion Sort":
                    AddLegendItem(LegendGrid, "Current Element", Colors.Red, currentRow++);
                    AddLegendItem(LegendGrid, "Current Comparison", Colors.Orange, currentRow++);
                    AddLegendItem(LegendGrid, "Sorted", Colors.BlueViolet, currentRow++);
                    break;

                default:
                    AddLegendItem(LegendGrid, "No specific legend available.", Colors.Gray, currentRow++);
                    break;
            }
        }

        private void AddLegendItem(Grid grid, string text, Color color, int row)
        {
            // Add a new row definition for each legend item
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create a square rounded color indicator
            Frame colorIndicator = new Frame
            {
                BackgroundColor = color,
                CornerRadius = 5, // Smaller value for rounded square corners
                WidthRequest = 20,
                HeightRequest = 20,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                HasShadow = false,
                Padding = 0
            };

            // Create the legend description label
            Label legendDescription = new Label
            {
                Text = text,
                FontSize = 14,
                FontFamily = "Arial", // Example font, customize as needed
                FontAttributes = FontAttributes.None,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(10, 0, 0, 0) // Add spacing between the color and text
            };

            // Add the color indicator to the grid
            Grid.SetRow(colorIndicator, row);
            Grid.SetColumn(colorIndicator, 0);
            grid.Children.Add(colorIndicator);

            // Add the legend description to the grid
            Grid.SetRow(legendDescription, row);
            Grid.SetColumn(legendDescription, 1);
            grid.Children.Add(legendDescription);
        }


    }
}

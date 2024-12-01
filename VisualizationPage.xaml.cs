using Microsoft.Maui.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SortingVisualizer.Extensions;



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
                "Insertion Sort" => "Insertion Sort is a simple, in-place comparison-based sorting algorithm that builds the final sorted array one item at a time by repeatedly inserting elements into their correct position within the sorted part of the array.",
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

        private async Task BubbleSortVisual(int[] array, CancellationToken cancellationToken)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                for (int j = 0; j < array.Length - i - 1; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Evidențiază elementele care sunt comparate - culoare portocaliu deschis
                    UpdateVisualizationGrid(array, j, j + 1, Colors.Orange);
                    await Task.Delay(1000, cancellationToken); // Proces lent pentru claritate (1 secundă între comparații)

                    if (array[j] > array[j + 1])
                    {
                        // Elementele care urmează să fie schimbate sunt evidențiate în continuare
                        UpdateVisualizationGrid(array, j, j + 1, Colors.Orange);

                        // Așteaptă pentru a face evidențierea clară
                        await Task.Delay(500, cancellationToken); // Pauză scurtă pentru evidențiere

                        // Animația de schimbare
                        await SmoothSwapBoubble(j, j + 1);

                        // Efectuează schimbul în array
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;

                        // Actualizează vizualizarea după schimb, fără a evidenția
                        UpdateVisualizationGrid(array);
                        await Task.Delay(500, cancellationToken); // Pauză scurtă după schimb (0.5 secunde)
                    }
                }

                // După fiecare iterație, elementul sortat este poziționat corect
                UpdateVisualizationGrid(array, -1, array.Length - i - 1, Colors.CornflowerBlue);
            }

            // Evidențierea finală a tuturor elementelor ca fiind sortate
            UpdateVisualizationGrid(array, -1, -1, Colors.CornflowerBlue);
        }

        private async Task SmoothSwapBoubble(int index1, int index2)
        {
            var element1 = VisualizationGrid.Children[index1] as StackLayout;
            var element2 = VisualizationGrid.Children[index2] as StackLayout;

            if (element1 != null && element2 != null)
            {
                double element1InitialPosition = element1.TranslationX;
                double element2InitialPosition = element2.TranslationX;

                double distanceBetweenElements = Math.Abs(element2InitialPosition - element1InitialPosition);
                double halfDistance = distanceBetweenElements / 2;

                // Faza 1: Mișcare spre centru, până când se suprapun parțial
                var moveElement1ToCenter = element1.TranslateTo(element1InitialPosition + halfDistance, 0, 400, Easing.CubicInOut);
                var moveElement2ToCenter = element2.TranslateTo(element2InitialPosition - halfDistance, 0, 400, Easing.CubicInOut);

                await Task.WhenAll(moveElement1ToCenter, moveElement2ToCenter);

                // Faza 2: Mișcare completă către pozițiile celuilalt element
                var moveElement1ToEnd = element1.TranslateTo(element2InitialPosition - element1InitialPosition, 0, 400, Easing.CubicInOut);
                var moveElement2ToEnd = element2.TranslateTo(element1InitialPosition - element2InitialPosition, 0, 400, Easing.CubicInOut);

                await Task.WhenAll(moveElement1ToEnd, moveElement2ToEnd);
            }
        }

        private async Task SelectionSortVisual(int[] array, CancellationToken cancellationToken)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                int minIndex = i;

                // Evidențiază elementul curent minim (culoare roșie)
                await UpdateVisualizationGridSelection(array, currentMinIndex: minIndex, sortedUntil: i - 1);
                await Task.Delay(500, cancellationToken);

                for (int j = i + 1; j < array.Length; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Evidențiază elementul comparat cu minimul (culoare LightCoral)
                    await UpdateVisualizationGridSelection(array, currentMinIndex: minIndex, comparedIndex: j, sortedUntil: i - 1);
                    await Task.Delay(500, cancellationToken);

                    if (array[j] < array[minIndex])
                    {
                        // Scoate evidențierea vechiului minim și evidențiază noul minim
                        minIndex = j;

                        // Evidențiază noul minim în roșu
                        await UpdateVisualizationGridSelection(array, currentMinIndex: minIndex, sortedUntil: i - 1);
                        await Task.Delay(500, cancellationToken);
                    }
                }

                // Dacă minimul găsit nu este la poziția inițială, efectuează schimbul
                if (minIndex != i)
                {
                    // Evidențiază elementele care vor fi schimbate fără a le colora în roșu
                    await UpdateVisualizationGridSelection(array, currentMinIndex: -1, comparedIndex: -1, sortedUntil: i - 1);
                    await Task.Delay(500, cancellationToken);

                    // Animația de schimbare
                    await SmoothSwapSelection(i, minIndex);

                    // Efectuează schimbul în array
                    int temp = array[i];
                    array[i] = array[minIndex];
                    array[minIndex] = temp;

                    // Evidențiază partea sortată în albastru deschis (inclusiv elementul curent sortat)
                    await UpdateVisualizationGridSelection(array, sortedUntil: i);
                    await Task.Delay(500, cancellationToken);
                }
                else
                {
                    // Evidențiază partea sortată în albastru deschis (în cazul în care nu a avut loc schimb)
                    await UpdateVisualizationGridSelection(array, sortedUntil: i);
                    await Task.Delay(500, cancellationToken);
                }
            }

            // Evidențierea finală a tuturor elementelor ca fiind sortate în albastru deschis
            await UpdateVisualizationGridSelection(array, sortedUntil: array.Length - 1);
        }

        private async Task SmoothSwapSelection(int index1, int index2)
        {
            var element1 = VisualizationGrid.Children[index1] as StackLayout;
            var element2 = VisualizationGrid.Children[index2] as StackLayout;

            if (element1 != null && element2 != null)
            {
                // Determină pozițiile inițiale
                double element1InitialPosition = element1.TranslationX;
                double element2InitialPosition = element2.TranslationX;

                // Distanța dintre cele două elemente
                double distanceBetweenElements = Math.Abs(element2InitialPosition - element1InitialPosition);

                // Calcularea punctului de suprapunere (jumătatea distanței dintre elemente)
                double halfDistance = distanceBetweenElements / 2;

                // Faza 1: Mișcarea simultană spre punctul de suprapunere
                var moveElement1ToCenter = element1.TranslateTo(element1InitialPosition + halfDistance, 0, 400, Easing.CubicInOut);
                var moveElement2ToCenter = element2.TranslateTo(element2InitialPosition - halfDistance, 0, 400, Easing.CubicInOut);

                await Task.WhenAll(moveElement1ToCenter, moveElement2ToCenter);

                // Faza 2: Mișcarea completă spre pozițiile finale (schimbul locurilor)
                var moveElement1ToEnd = element1.TranslateTo(element2InitialPosition - element1InitialPosition, 0, 400, Easing.CubicInOut);
                var moveElement2ToEnd = element2.TranslateTo(element1InitialPosition - element2InitialPosition, 0, 400, Easing.CubicInOut);

                await Task.WhenAll(moveElement1ToEnd, moveElement2ToEnd);
            }
        }

        private async Task UpdateVisualizationGridSelection(int[] data, int currentMinIndex = -1, int comparedIndex = -1, int sortedUntil = -1)
        {
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300;
            int maxDataValue = data.Max();

            // Adaugă elementele doar dacă nu sunt deja adăugate
            if (VisualizationGrid.Children.Count == 0)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var stackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        HorizontalOptions = LayoutOptions.Center
                    };

                    double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;

                    var boxView = new BoxView
                    {
                        Color = Colors.CornflowerBlue, // Default color
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

            // Actualizează doar elementele necesare
            for (int i = 0; i < data.Length; i++)
            {
                var stackLayout = VisualizationGrid.Children[i] as StackLayout;
                if (stackLayout == null) continue;

                var boxView = stackLayout.Children[0] as BoxView;
                if (boxView == null) continue;

                var label = stackLayout.Children[1] as Label;
                if (label == null) continue;

                // Actualizează înălțimea și textul
                double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;
                boxView.HeightRequest = relativeHeight;
                label.Text = data[i].ToString();

                // Evidențierea minimului curent (culoare roșie)
                if (i == currentMinIndex)
                {
                    boxView.Color = Colors.Red;
                }
                // Evidențierea elementului comparat cu minimul (culoare LightCoral)
                else if (i == comparedIndex)
                {
                    boxView.Color = Colors.LightCoral;
                }
                // Evidențierea elementelor deja sortate în albastru deschis
                else if (i <= sortedUntil)
                {
                    boxView.Color = Colors.LightBlue;
                }
                // Setează culoarea implicită pentru elementele nesortate și ne-evidențiate
                else
                {
                    boxView.Color = Colors.CornflowerBlue;
                }
            }

            await Task.CompletedTask;
        }

        private async Task MergeSortVisual(int[] array, int left, int right, CancellationToken cancellationToken, int depth = 0)
        {
            if (left < right)
            {
                // Highlight the current sub-array being processed
                await HighlightSubArray(array, left, right);
                await Task.Delay(500, cancellationToken);

                int middle = (left + right) / 2;

                // Recursively sort the left and right halves
                await MergeSortVisual(array, left, middle, cancellationToken, depth + 1);
                await MergeSortVisual(array, middle + 1, right, cancellationToken, depth + 1);

                // Merge the sorted halves
                await Merge(array, left, middle, right, cancellationToken);
            }
        }

        private async Task Merge(int[] array, int left, int middle, int right, CancellationToken cancellationToken)
        {
            // Create temporary arrays to hold the two halves
            int n1 = middle - left + 1;
            int n2 = right - middle;

            int[] leftArray = new int[n1];
            int[] rightArray = new int[n2];

            // Copy data to temporary arrays
            for (int i = 0; i < n1; ++i)
                leftArray[i] = array[left + i];
            for (int j = 0; j < n2; ++j)
                rightArray[j] = array[middle + 1 + j];

            // Initial indexes of first and second subarrays
            int iLeft = 0, iRight = 0;

            // Initial index of merged subarry
            int k = left;

            // Merge the temp arrays
            while (iLeft < n1 && iRight < n2)
            {
                cancellationToken.ThrowIfCancellationRequested();

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

                // Update visualization for each element placement
                await UpdateVisualizationGridMergeImproved(array, left, right);
                await Task.Delay(500, cancellationToken);

                k++;
            }

            // Copy remaining elements of leftArray if any
            while (iLeft < n1)
            {
                cancellationToken.ThrowIfCancellationRequested();
                array[k] = leftArray[iLeft];
                iLeft++;
                k++;

                await UpdateVisualizationGridMergeImproved(array, left, right);
                await Task.Delay(500, cancellationToken);
            }

            // Copy remaining elements of rightArray if any
            while (iRight < n2)
            {
                cancellationToken.ThrowIfCancellationRequested();
                array[k] = rightArray[iRight];
                iRight++;
                k++;

                await UpdateVisualizationGridMergeImproved(array, left, right);
                await Task.Delay(500, cancellationToken);
            }
        }

        private async Task HighlightSubArray(int[] data, int left, int right)
        {
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
                        // Highlight the current sub-array
                        if (i >= left && i <= right)
                        {
                            boxView.Color = Colors.LightBlue;
                            label.TextColor = Colors.Black;
                        }
                        else
                        {
                            boxView.Color = Colors.LightGray;
                            label.TextColor = Colors.DarkGray;
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task UpdateVisualizationGridMergeImproved(int[] data, int left, int right)
        {
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300;
            int maxDataValue = data.Max();

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
                        double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;

                        boxView.HeightRequest = relativeHeight;
                        label.Text = data[i].ToString();

                        // Color logic for current sub-array
                        if (i >= left && i <= right)
                        {
                            boxView.Color = Colors.LightBlue;
                            label.TextColor = Colors.Black;
                        }
                        else
                        {
                            boxView.Color = Colors.LightGray;
                            label.TextColor = Colors.DarkGray;
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }


        private HashSet<int> sortedIndices = new HashSet<int>();

        public async Task QuickSortVisual(int[] array, int left, int right, CancellationToken cancellationToken)
        {
            if (left == 0 && right == array.Length - 1) // Clear sorted indices at the beginning of sorting
            {
                sortedIndices.Clear(); // Clear the sorted indices set
            }

            if (left < right)
            {
                // Partition the array and get the pivot index
                int pivotIndex = await Partition(array, left, right, cancellationToken);

                // Highlight the sorted pivot element and add to sorted indices
                sortedIndices.Add(pivotIndex);
                UpdateVisualizationGridQuick(array, pivotIndex, pivotIndex, Colors.LightBlue);
                await Task.Delay(500, cancellationToken);

                // Recursively sort elements before and after partition
                await QuickSortVisual(array, left, pivotIndex - 1, cancellationToken);
                await QuickSortVisual(array, pivotIndex + 1, right, cancellationToken);
            }
            else if (left == right) // Handles the case where a single element is sorted
            {
                // When left == right, it means the element is already in the correct place.
                sortedIndices.Add(left);
                UpdateVisualizationGridQuick(array, left, left, Colors.LightBlue);
            }
        }

        private async Task<int> Partition(int[] array, int left, int right, CancellationToken cancellationToken)
        {
            int pivot = array[right];
            int i = left - 1;

            // Highlight pivot element ONCE at the start
            UpdateVisualizationGridQuick(array, right, right, Colors.Red);
            await Task.Delay(500, cancellationToken);

            for (int j = left; j < right; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Highlight current comparison with a different color
                UpdateVisualizationGridQuick(array, j, j, Colors.LightCoral);
                await Task.Delay(500, cancellationToken);

                if (array[j] <= pivot)
                {
                    i++;
                    await SmoothSwap(i, j);

                    // Perform actual swap in the array
                    (array[i], array[j]) = (array[j], array[i]);

                    // Update visualization after swap
                    UpdateVisualizationGridQuick(array);
                    await Task.Delay(500, cancellationToken);
                }
            }

            // Swap array[i + 1] and array[right] to put pivot in correct position
            await SmoothSwap(i + 1, right);
            (array[i + 1], array[right]) = (array[right], array[i + 1]);

            // Highlight the pivot element as sorted
            sortedIndices.Add(i + 1);
            UpdateVisualizationGridQuick(array, i + 1, i + 1, Colors.LightBlue);
            await Task.Delay(500, cancellationToken);

            return i + 1;
        }

        private async Task UpdateVisualizationGridQuick(int[] data, int left = -1, int right = -1, Color highlightColor = default)
        {
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300;
            int maxDataValue = data.Max();

            // Update only if the grid is already populated
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

                // Set color based on sorted indices or current highlighting
                if (sortedIndices.Contains(i))
                {
                    boxView.Color = Colors.LightBlue; // Elements that are sorted remain LightBlue
                }
                else if (left != -1 && i >= left && i <= right)
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

        public async Task InsertionSortVisual(int[] array, CancellationToken cancellationToken)
        {
            for (int i = 1; i < array.Length; i++)
            {
                int key = array[i];
                int j = i - 1;

                // Highlight the key element being inserted
                await UpdateVisualizationGridInsertion(array, i, i, Colors.LightCoral);
                await Task.Delay(500, cancellationToken);

                // Shift elements of the sorted portion to the right to make space for the key element
                while (j >= 0 && array[j] > key)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Highlight the element being compared to the key
                    await UpdateVisualizationGridInsertion(array, j, j, Colors.CornflowerBlue);
                    await Task.Delay(500, cancellationToken);

                    // Swap elements to simulate shifting
                    (array[j + 1], array[j]) = (array[j], array[j + 1]);

                    // Update visualization after swapping the element
                    await UpdateVisualizationGrid(array, j, j + 1, Colors.CornflowerBlue);
                    await Task.Delay(500, cancellationToken);

                    j--;
                }

                // Insert the key element in the correct position
                array[j + 1] = key;
                await UpdateVisualizationGridInsertion(array, j + 1, j + 1, Colors.CornflowerBlue);
                await Task.Delay(500, cancellationToken);
            }

            // Mark all elements as sorted
            await UpdateVisualizationGridInsertion(array);
            await Task.Delay(500, cancellationToken);
        }

        private async Task UpdateVisualizationGridInsertion(int[] data, int left = -1, int right = -1, Color highlightColor = default)
        {
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300;
            int maxDataValue = data.Max();

            // Update only if the grid is already populated
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

                // Set color based on current highlighting
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

        private async Task SmoothSwap(int index1, int index2)
        {
            if (index1 == index2)
                return;

            var element1 = VisualizationGrid.Children[index1] as StackLayout;
            var element2 = VisualizationGrid.Children[index2] as StackLayout;

            if (element1 != null && element2 != null)
            {
                double element1InitialPosition = element1.TranslationX;
                double element2InitialPosition = element2.TranslationX;

                // Animate the movement to swap positions
                var moveElement1 = element1.TranslateTo(element2InitialPosition - element1InitialPosition, 0, 500, Easing.CubicInOut);
                var moveElement2 = element2.TranslateTo(element1InitialPosition - element2InitialPosition, 0, 500, Easing.CubicInOut);

                await Task.WhenAll(moveElement1, moveElement2);
            }
        }



        private async Task UpdateVisualizationGrid(int[] data, int left = -1, int right = -1, Color highlightColor = default, string sortingType = "")
        {
            double maxHeight = VisualizationGrid.Height > 0 ? VisualizationGrid.Height : 300;
            int maxDataValue = data.Max();

            // Adaugă elementele doar dacă nu sunt deja adăugate
            if (VisualizationGrid.Children.Count == 0)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var stackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Vertical,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        HorizontalOptions = LayoutOptions.Center
                    };

                    double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;

                    var boxView = new BoxView
                    {
                        Color = Colors.CornflowerBlue, // Default color
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

            // Actualizează doar elementele necesare
            for (int i = 0; i < data.Length; i++)
            {
                var stackLayout = VisualizationGrid.Children[i] as StackLayout;
                if (stackLayout == null) continue;

                var boxView = stackLayout.Children[0] as BoxView;
                if (boxView == null) continue;

                var label = stackLayout.Children[1] as Label;
                if (label == null) continue;

                // Actualizează înălțimea și textul
                double relativeHeight = maxDataValue > 0 ? (data[i] / (double)maxDataValue) * (maxHeight * 0.9) : 0;
                boxView.HeightRequest = relativeHeight;
                label.Text = data[i].ToString();

                // Evidențierea pe baza tipului de sortare
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
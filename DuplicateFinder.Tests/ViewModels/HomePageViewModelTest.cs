﻿using DuplicateFinder.Framework;
using DuplicateFinder.Models;
using DuplicateFinder.Utilities;
using DuplicateFinder.ViewModels;
using FileHashRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Forms = System.Windows.Forms;

namespace DuplicateFinder.Tests.ViewModels
{
    [TestClass]
    public class HomePageViewModelTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            App.NavigationService = new Mock<IPageNavigationService>().Object;
        }

        [TestMethod]
        public async Task PageLoaded_LoadsScannedFileStoreFromFile()
        {
            // ARRANGE
            string selectedLocation = "C:\\foobar";
            Mock<IScannedFileStore> scannedFileStore = GetMockScannedFileStore();
            HomePageViewModel viewModel = new HomePageViewModel(
                GetMockFolderBrowserDialogWrapper(selectedLocation).Object,
                scannedFileStore.Object,
                "C:\\user");

            // ACT
            await viewModel.PageLoaded.ExecuteAsync(null);

            // ASSERT
            scannedFileStore.Verify(t => t.LoadScannedFileStoreFromFileAsync(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void Add_AddsLocationsToModel()
        {
            // ARRANGE
            string selectedLocation = "C:\\foobar";
            HomePageViewModel viewModel = new HomePageViewModel(
                GetMockFolderBrowserDialogWrapper(selectedLocation).Object,
                GetMockScannedFileStore().Object,
                "C:\\user");

            // ACT
            viewModel.Add.Execute(null);

            // ASSERT
            Assert.AreEqual(selectedLocation, viewModel.Locations[0].Path);
        }

        [TestMethod]
        public void Add_IgnoresDuplicateLocations()
        {
            // ARRANGE
            string selectedLocation = "C:\\foobar";
            HomePageViewModel viewModel = new HomePageViewModel(
                GetMockFolderBrowserDialogWrapper(selectedLocation).Object, 
                GetMockScannedFileStore().Object,
                "C:\\user");

            // ACT
            viewModel.Add.Execute(null);

            // ASSERT
            Assert.AreEqual(1, viewModel.Locations.Count, "Item was added more than once to the Model.");
        }

        [TestMethod]
        public void Add_NoLocationSelected_DoesNothing()
        {

            // ARRANGE
            string selectedLocation = "C:\\foobar";
            var mockFolderBrowserDialog = GetMockFolderBrowserDialogWrapper(selectedLocation);
            HomePageViewModel viewModel = new HomePageViewModel(
                mockFolderBrowserDialog.Object,
                GetMockScannedFileStore().Object,
                "C:\\user");
            mockFolderBrowserDialog.Setup(t => t.ShowDialogWrapper(It.IsAny<Forms.CommonDialog>())).Returns<Forms.CommonDialog>(t =>
            {
                return Forms.DialogResult.Cancel;
            });

            // ACT
            viewModel.Add.Execute(null);

            // ASSERT
            Assert.AreEqual(0, viewModel.Locations.Count, "Item was added to the Model.");
        }

        [TestMethod]
        public void Add_TogglesButtonsWhenLocationsAdded()
        {
            // ARRANGE
            string selectedLocation = "C:\\foobar";
            HomePageViewModel viewModel = new HomePageViewModel(
                GetMockFolderBrowserDialogWrapper(selectedLocation).Object,
                GetMockScannedFileStore().Object,
                "C:\\user");

            // ACT
            viewModel.Add.Execute(null);

            // ASSERT
            Assert.IsTrue(viewModel.Remove.CanExecute(null), "Remove is not enabled.");
            Assert.IsTrue(viewModel.Scan.CanExecute(null), "Scan is not enabled.");
        }

        [TestMethod]
        public void Remove_RemovesLocationFromPageAndModel()
        {
            // ARRANGE
            ScanLocation location = new ScanLocation("C:\\foobar");
            HomePageViewModel viewModel = new HomePageViewModel(
                GetMockFolderBrowserDialogWrapper(string.Empty).Object,
                GetMockScannedFileStore().Object,
                "C:\\user");
            viewModel.Locations.Add(location);

            // ACT
            viewModel.SelectedLocation = location;
            viewModel.Remove.Execute(null);

            // ASSERT
            Assert.AreEqual(0, viewModel.Locations.Count, "Location was still in the Model.");
        }

        [TestMethod]
        public void Remove_NoLocationSelected_DoesNothing()
        {
            // ARRANGE
            ScanLocation location = new ScanLocation("C:\\foobar");
            HomePageViewModel viewModel = new HomePageViewModel(
                GetMockFolderBrowserDialogWrapper(string.Empty).Object,
                GetMockScannedFileStore().Object,
                "C:\\user");
            viewModel.Locations.Add(location);

            // ACT
            viewModel.SelectedLocation = null;
            viewModel.Remove.Execute(null);

            // ASSERT
            Assert.AreEqual(1, viewModel.Locations.Count, "Item was removed from the Model.");
        }

        [TestMethod]
        public void Remove_TogglesButtonsWhenLocationRemoved()
        {
            // ARRANGE
            ScanLocation location = new ScanLocation("C:\\foobar");
            HomePageViewModel viewModel = new HomePageViewModel(
                GetMockFolderBrowserDialogWrapper(string.Empty).Object,
                GetMockScannedFileStore().Object,
                "C:\\user");
            viewModel.Locations.Add(location);

            // ACT
            viewModel.SelectedLocation = location;
            viewModel.Remove.Execute(null);

            // ASSERT
            Assert.IsFalse(viewModel.Remove.CanExecute(null), "The RemoveButton is enabled.");
            Assert.IsFalse(viewModel.Scan.CanExecute(null), "The ScanButton is enabled.");
        }

        [TestMethod]
        public void Scan_NavigatesPage()
        {
            // ARRANGE
            var mockNavigationService = new Mock<IPageNavigationService>();
            App.NavigationService = mockNavigationService.Object;
            HomePageViewModel viewModel = new HomePageViewModel(
                GetMockFolderBrowserDialogWrapper(string.Empty).Object,
                GetMockScannedFileStore().Object,
                "C:\\user");

            // ACT
            viewModel.Scan.Execute(null);

            // ASSERT
            mockNavigationService.Verify(t => t.Navigate(It.IsAny<object>()), "The NavigationService failed to navigate anywhere");

        }

        private Mock<IFolderBrowserDialogWrapper> GetMockFolderBrowserDialogWrapper(string selectedPath)
        {
            Mock<IFolderBrowserDialogWrapper> wrapper = new Mock<IFolderBrowserDialogWrapper>();
            Mock<Forms.CommonDialog> dialog = new Mock<Forms.CommonDialog>();
            wrapper.Setup(t => t.GetNewFolderBrowserDialog()).Returns(() =>
            {
                return dialog.Object;
            });
            wrapper.Setup(t => t.ShowDialogWrapper(It.IsAny<Forms.CommonDialog>())).Returns(() =>
            {
                return Forms.DialogResult.OK;
            });
            wrapper.Setup(t => t.GetSelectedPathFromDialog(It.IsAny<Forms.CommonDialog>())).Returns<Forms.CommonDialog>(t =>
            {
                return selectedPath;
            });

            return wrapper;
        }

        private Mock<IScannedFileStore> GetMockScannedFileStore()
        {
            return new Mock<IScannedFileStore>();
        }
    }
}

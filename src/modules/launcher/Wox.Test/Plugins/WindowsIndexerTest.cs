﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Plugin.Indexer;
using Microsoft.Plugin.Indexer.DriveDetection;
using Microsoft.Plugin.Indexer.SearchHelper;
using Microsoft.Search.Interop;
using Moq;
using NUnit.Framework;
using Wox.Plugin;

namespace Wox.Test.Plugins
{
    [TestFixture]
    public class WindowsIndexerTest
    {
        private WindowsSearchAPI GetWindowsSearchAPI()
        {
            var mock = new Mock<ISearch>();
            mock.Setup(x => x.Query("dummy-connection-string", "dummy-query")).Returns(new List<OleDBResult>());
            return new WindowsSearchAPI(mock.Object);
        }

        private ISearchManager GetMockSearchManager()
        {
            var sqlQuery = "SELECT TOP 30 \"System.ItemUrl\", \"System.FileName\", \"System.FileAttributes\" FROM \"SystemIndex\" WHERE CONTAINS(System.FileName,'\"FilePath\"',1033) AND scope='file:' ORDER BY System.DateModified DESC";
            var mockSearchManager = new Mock<ISearchManager>();
            var mockCatalog = new Mock<CSearchCatalogManager>();
            var mockQueryHelper = new Mock<CSearchQueryHelper>();
            mockQueryHelper.SetupAllProperties();
            mockQueryHelper.Setup(x => x.ConnectionString).Returns("provider=Search.CollatorDSO.1;EXTENDED PROPERTIES=\"Application=Windows\"");
            mockQueryHelper.Setup(x => x.GenerateSQLFromUserQuery(It.IsAny<string>())).Returns(sqlQuery);
            mockSearchManager.Setup(x => x.GetCatalog(It.IsAny<string>())).Returns(mockCatalog.Object);
            mockCatalog.Setup(x => x.GetQueryHelper()).Returns(mockQueryHelper.Object);
            return mockSearchManager.Object;
        }

        [Test]
        public void InitQueryHelperShouldInitializeWhenFunctionIsCalled()
        {
            // Arrange
            int maxCount = 10;
            WindowsSearchAPI api = GetWindowsSearchAPI();
            ISearchQueryHelper queryHelper = null;
            var mockSearchManager = GetMockSearchManager();

            // Act
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, maxCount, api.DisplayHiddenFiles);

            // Assert
            Assert.IsNotNull(queryHelper);
            Assert.AreEqual(maxCount, queryHelper.QueryMaxResults);
        }

        [Test]
        public void ModifyQueryHelperShouldSetQueryHelperWhenPatternIsAsterisk()
        {
            // Arrange
            ISearchQueryHelper queryHelper;
            string pattern = "*";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            var mockSearchManager = GetMockSearchManager();
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);

            // Act
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("LIKE", StringComparison.InvariantCulture));
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("Contains", StringComparison.InvariantCulture));
        }

        [Test]
        public void ModifyQueryHelperShouldSetQueryHelperWhenPatternContainsAsterisk()
        {
            // Arrange
            ISearchQueryHelper queryHelper;
            string pattern = "tt*^&)";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            var mockSearchManager = GetMockSearchManager();
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);

            // Act
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsTrue(queryHelper.QueryWhereRestrictions.Contains("LIKE", StringComparison.InvariantCulture));
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("Contains", StringComparison.InvariantCulture));
        }

        [Test]
        public void ModifyQueryHelperShouldSetQueryHelperWhenPatternContainsPercent()
        {
            // Arrange
            ISearchQueryHelper queryHelper;
            string pattern = "tt%^&)";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            var mockSearchManager = GetMockSearchManager();
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);

            // Act
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsTrue(queryHelper.QueryWhereRestrictions.Contains("LIKE", StringComparison.InvariantCulture));
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("Contains", StringComparison.InvariantCulture));
        }

        [Test]
        public void ModifyQueryHelperShouldSetQueryHelperWhenPatternContainsUnderScore()
        {
            // Arrange
            ISearchQueryHelper queryHelper;
            string pattern = "tt_^&)";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            var mockSearchManager = GetMockSearchManager();
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);

            // Act
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsTrue(queryHelper.QueryWhereRestrictions.Contains("LIKE", StringComparison.InvariantCulture));
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("Contains", StringComparison.InvariantCulture));
        }

        [Test]
        public void ModifyQueryHelperShouldSetQueryHelperWhenPatternContainsQuestionMark()
        {
            // Arrange
            ISearchQueryHelper queryHelper;
            string pattern = "tt?^&)";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            var mockSearchManager = GetMockSearchManager();
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);

            // Act
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsTrue(queryHelper.QueryWhereRestrictions.Contains("LIKE", StringComparison.InvariantCulture));
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("Contains", StringComparison.InvariantCulture));
        }

        [Test]
        public void ModifyQueryHelperShouldSetQueryHelperWhenPatternDoesNotContainSplSymbols()
        {
            // Arrange
            ISearchQueryHelper queryHelper;
            string pattern = "tt^&)bc";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            var mockSearchManager = GetMockSearchManager();
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);

            // Act
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("LIKE", StringComparison.InvariantCulture));
            Assert.IsTrue(queryHelper.QueryWhereRestrictions.Contains("Contains", StringComparison.InvariantCulture));
        }

        [Test]
        public void WindowsSearchAPIShouldReturnResultsWhenSearchWasExecuted()
        {
            // Arrange
            OleDBResult unHiddenFile = new OleDBResult(new List<object>() { "C:/test/path/file1.txt", "file1.txt" });
            OleDBResult hiddenFile = new OleDBResult(new List<object>() { "C:/test/path/file2.txt", "file2.txt" });
            List<OleDBResult> results = new List<OleDBResult>() { hiddenFile, unHiddenFile };
            var mock = new Mock<ISearch>();
            mock.Setup(x => x.Query(It.IsAny<string>(), It.IsAny<string>())).Returns(results);
            WindowsSearchAPI api = new WindowsSearchAPI(mock.Object, true);
            var mockSearchManager = GetMockSearchManager();

            // Act
            var windowsSearchAPIResults = api.Search("FilePath", mockSearchManager);

            // Assert
            Assert.IsTrue(windowsSearchAPIResults.Count() == 2);
            Assert.IsTrue(windowsSearchAPIResults.Any(x => x.Title == "file1.txt"));
            Assert.IsTrue(windowsSearchAPIResults.Any(x => x.Title == "file2.txt"));
        }

        [Test]
        public void WindowsSearchAPIShouldNotReturnResultsWithNullValueWhenDbResultHasANullColumn()
        {
            // Arrange
            OleDBResult unHiddenFile = new OleDBResult(new List<object>() { "C:/test/path/file1.txt", DBNull.Value });
            OleDBResult hiddenFile = new OleDBResult(new List<object>() { "C:/test/path/file2.txt", "file2.txt" });
            List<OleDBResult> results = new List<OleDBResult>() { hiddenFile, unHiddenFile };
            var mock = new Mock<ISearch>();
            mock.Setup(x => x.Query(It.IsAny<string>(), It.IsAny<string>())).Returns(results);
            WindowsSearchAPI api = new WindowsSearchAPI(mock.Object, false);
            var mockSearchManager = GetMockSearchManager();

            // Act
            var windowsSearchAPIResults = api.Search("FilePath", mockSearchManager);

            // Assert
            Assert.IsTrue(windowsSearchAPIResults.Count() == 1);
            Assert.IsFalse(windowsSearchAPIResults.Any(x => x.Title == "file1.txt"));
            Assert.IsTrue(windowsSearchAPIResults.Any(x => x.Title == "file2.txt"));
        }

        [Test]
        public void WindowsSearchAPIShouldRequestNormalRequestWhenDisplayHiddenFilesIsTrue()
        {
            ISearchQueryHelper queryHelper;
            string pattern = "notepad";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            api.DisplayHiddenFiles = true;
            var mockSearchManager = GetMockSearchManager();

            // Act
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("AND System.FileAttributes <> SOME BITWISE 2", StringComparison.InvariantCulture));
        }

        [Test]
        public void WindowsSearchAPIShouldRequestFilteredRequestWhenDisplayHiddenFilesIsFalse()
        {
            ISearchQueryHelper queryHelper;
            string pattern = "notepad";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            api.DisplayHiddenFiles = false;
            var mockSearchManager = GetMockSearchManager();

            // Act
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsTrue(queryHelper.QueryWhereRestrictions.Contains("AND System.FileAttributes <> SOME BITWISE 2", StringComparison.InvariantCulture));
        }

        [Test]
        public void WindowsSearchAPIShouldRequestNormalRequestWhenDisplayHiddenFilesIsTrueAfterRuntimeSwap()
        {
            ISearchQueryHelper queryHelper;
            string pattern = "notepad";
            WindowsSearchAPI api = GetWindowsSearchAPI();
            api.DisplayHiddenFiles = false;
            var mockSearchManager = GetMockSearchManager();

            // Act
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);
            api.DisplayHiddenFiles = true;
            WindowsSearchAPI.InitQueryHelper(out queryHelper, mockSearchManager, 10, api.DisplayHiddenFiles);
            WindowsSearchAPI.ModifyQueryHelper(ref queryHelper, pattern);

            // Assert
            // Using InvariantCulture since this is a command line arg
            Assert.IsFalse(queryHelper.QueryWhereRestrictions.Contains("AND System.FileAttributes <> SOME BITWISE 2", StringComparison.InvariantCulture));
        }

        [TestCase("item.exe")]
        [TestCase("item.bat")]
        [TestCase("item.appref-ms")]
        [TestCase("item.lnk")]
        public void LoadContextMenusMustLoadAllItemsWhenFileIsAnApp(string path)
        {
            // Arrange
            var mockapi = new Mock<IPublicAPI>();
            var pluginInitContext = new PluginInitContext() { API = mockapi.Object };

            ContextMenuLoader contextMenuLoader = new ContextMenuLoader(pluginInitContext);

            // Act
            Result result = new Result
            {
                ContextData = new SearchResult { Path = path },
            };

            List<ContextMenuResult> contextMenuItems = contextMenuLoader.LoadContextMenus(result);

            // Assert
            Assert.AreEqual(4, contextMenuItems.Count);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_open_containing_folder, contextMenuItems[0].Title);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_run_as_administrator, contextMenuItems[1].Title);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_copy_path, contextMenuItems[2].Title);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_open_in_console, contextMenuItems[3].Title);
        }

        [TestCase("item.pdf")]
        [TestCase("item.xls")]
        [TestCase("item.ppt")]
        [TestCase("C:/DummyFile.cs")]
        public void LoadContextMenusMustNotLoadRunAsAdminWhenFileIsAnNotApp(string path)
        {
            // Arrange
            var mockapi = new Mock<IPublicAPI>();
            var pluginInitContext = new PluginInitContext() { API = mockapi.Object };

            ContextMenuLoader contextMenuLoader = new ContextMenuLoader(pluginInitContext);

            // Act
            Result result = new Result
            {
                ContextData = new SearchResult { Path = path },
            };

            List<ContextMenuResult> contextMenuItems = contextMenuLoader.LoadContextMenus(result);

            // Assert
            Assert.AreEqual(3, contextMenuItems.Count);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_open_containing_folder, contextMenuItems[0].Title);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_copy_path, contextMenuItems[1].Title);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_open_in_console, contextMenuItems[2].Title);
        }

        [TestCase("C:/DummyFolder")]
        [TestCase("TestFolder")]
        public void LoadContextMenusMustNotLoadRunAsAdminAndOpenContainingFolderForFolder(string path)
        {
            // Arrange
            var mockapi = new Mock<IPublicAPI>();
            var pluginInitContext = new PluginInitContext() { API = mockapi.Object };

            ContextMenuLoader contextMenuLoader = new ContextMenuLoader(pluginInitContext);

            // Act
            Result result = new Result
            {
                ContextData = new SearchResult { Path = path },
            };

            List<ContextMenuResult> contextMenuItems = contextMenuLoader.LoadContextMenus(result);

            // Assert
            Assert.AreEqual(2, contextMenuItems.Count);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_copy_path, contextMenuItems[0].Title);
            Assert.AreEqual(Microsoft.Plugin.Indexer.Properties.Resources.Microsoft_plugin_indexer_open_in_console, contextMenuItems[1].Title);
        }

        [TestCase(0, false, ExpectedResult = true)]
        [TestCase(0, true, ExpectedResult = false)]
        [TestCase(1, false, ExpectedResult = false)]
        [TestCase(1, true, ExpectedResult = false)]
        public bool DriveDetectionMustDisplayWarningWhenEnhancedModeIsOffAndWhenWarningIsNotDisabled(int enhancedModeStatus, bool disableWarningCheckBoxStatus)
        {
            // Arrange
            var mockRegistry = new Mock<IRegistryWrapper>();
            mockRegistry.Setup(r => r.GetHKLMRegistryValue(It.IsAny<string>(), It.IsAny<string>())).Returns(enhancedModeStatus); // Enhanced mode is disabled

            IndexerDriveDetection driveDetection = new IndexerDriveDetection(mockRegistry.Object);
            driveDetection.IsDriveDetectionWarningCheckBoxSelected = disableWarningCheckBoxStatus;

            // Act & Assert
            return driveDetection.DisplayWarning();
        }

        [Test]
        public void SimplifyQueryShouldRemoveLikeQueryWhenSQLQueryUsesLIKESyntax()
        {
            // Arrange
            string sqlQuery = "SELECT TOP 30 \"System.ItemUrl\", \"System.FileName\", \"System.FileAttributes\" FROM \"SystemIndex\" WHERE (System.FileName LIKE 'abcd.%' OR CONTAINS(System.FileName,'\"abcd.*\"',1033)) AND scope='file:' ORDER BY System.DateModified DESC";

            // Act
            var simplifiedSqlQuery = WindowsSearchAPI.SimplifyQuery(sqlQuery);

            // Assert
            string expectedSqlQuery = "SELECT TOP 30 \"System.ItemUrl\", \"System.FileName\", \"System.FileAttributes\" FROM \"SystemIndex\" WHERE (CONTAINS(System.FileName,'\"abcd.*\"',1033)) AND scope='file:' ORDER BY System.DateModified DESC";
            Assert.IsFalse(simplifiedSqlQuery.Equals(sqlQuery, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(simplifiedSqlQuery.Equals(expectedSqlQuery, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void SimplifyQueryShouldReturnArgumentWhenSQLQueryDoesNotUseLIKESyntax()
        {
            // Arrange
            string sqlQuery = "SELECT TOP 30 \"System.ItemUrl\", \"System.FileName\", \"System.FileAttributes\" FROM \"SystemIndex\" WHERE CONTAINS(System.FileName,'\"abcd*\"',1033) AND scope='file:' ORDER BY System.DateModified DESC";

            // Act
            var simplifiedSqlQuery = WindowsSearchAPI.SimplifyQuery(sqlQuery);

            // Assert
            Assert.IsTrue(simplifiedSqlQuery.Equals(sqlQuery, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void SimplifyQueryShouldRemoveAllOccurrencesOfLikeQueryWhenSQLQueryUsesLIKESyntaxMultipleTimes()
        {
            // Arrange
            string sqlQuery = "SELECT TOP 30 \"System.ItemUrl\", \"System.FileName\", \"System.FileAttributes\", \"System.FileExtension\" FROM \"SystemIndex\" WHERE (System.FileName LIKE 'ab.%' OR CONTAINS(System.FileName,'\"ab.*\"',1033)) AND (System.FileExtension LIKE '.cd%' OR CONTAINS(System.FileName,'\".cd*\"',1033)) AND scope='file:' ORDER BY System.DateModified DESC";

            // Act
            var simplifiedSqlQuery = WindowsSearchAPI.SimplifyQuery(sqlQuery);

            // Assert
            string expectedSqlQuery = "SELECT TOP 30 \"System.ItemUrl\", \"System.FileName\", \"System.FileAttributes\", \"System.FileExtension\" FROM \"SystemIndex\" WHERE (CONTAINS(System.FileName,'\"ab.*\"',1033)) AND (CONTAINS(System.FileName,'\".cd*\"',1033)) AND scope='file:' ORDER BY System.DateModified DESC";
            Assert.IsFalse(simplifiedSqlQuery.Equals(sqlQuery, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(simplifiedSqlQuery.Equals(expectedSqlQuery, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void SimplifyQueryShouldRemoveLikeQueryWhenSQLQueryUsesLIKESyntaxAndContainsEscapedSingleQuotationMarks()
        {
            // Arrange
            string sqlQuery = "SELECT TOP 30 \"System.ItemUrl\", \"System.FileName\", \"System.FileAttributes\" FROM \"SystemIndex\" WHERE (System.FileName LIKE '''ab.cd''%' OR CONTAINS(System.FileName,'\"'ab.cd'*\"',1033)) AND scope='file:' ORDER BY System.DateModified DESC";

            // Act
            var simplifiedSqlQuery = WindowsSearchAPI.SimplifyQuery(sqlQuery);

            // Assert
            string expectedSqlQuery = "SELECT TOP 30 \"System.ItemUrl\", \"System.FileName\", \"System.FileAttributes\" FROM \"SystemIndex\" WHERE (CONTAINS(System.FileName,'\"'ab.cd'*\"',1033)) AND scope='file:' ORDER BY System.DateModified DESC";
            Assert.IsFalse(simplifiedSqlQuery.Equals(sqlQuery, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(simplifiedSqlQuery.Equals(expectedSqlQuery, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void WindowsSearchAPIShouldReturnEmptyResultsWhenIsFullQueryIsTrueAndTheQueryDoesNotRequireLIKESyntax()
        {
            // Arrange
            OleDBResult file1 = new OleDBResult(new List<object>() { "C:/test/path/file1.txt", DBNull.Value });
            OleDBResult file2 = new OleDBResult(new List<object>() { "C:/test/path/file2.txt", "file2.txt" });

            List<OleDBResult> results = new List<OleDBResult>() { file1, file2 };
            var mock = new Mock<ISearch>();
            mock.Setup(x => x.Query(It.IsAny<string>(), It.IsAny<string>())).Returns(results);
            WindowsSearchAPI api = new WindowsSearchAPI(mock.Object, false);
            var searchManager = GetMockSearchManager();

            // Act
            var windowsSearchAPIResults = api.Search("file", searchManager, true);

            // Assert
            Assert.IsTrue(windowsSearchAPIResults.Count() == 0);
        }
    }
}

﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using PlaylistManager.ApplicationServices;
using PlaylistManager.ApplicationServices.Models;
using PlaylistManager.ApplicationServices.Services;
using PlaylistManager.ApplicationServices.Services.Interfaces;
using PlaylistManager.WPF.ViewModels.Interfaces;

namespace PlaylistManager.WPF.ViewModels
{
    public class MainViewModel : PropertyChangedBase, IMainViewModel
    {
        private readonly ILibraryService _libraryService;
        private readonly IPlaylistService _playlistService;

        public ObservableCollection<AudioFile> AudioFiles
        {
            get
            {
                return new ObservableCollection<AudioFile>(_libraryService.LibraryFiles);
            }
        }

        public ObservableCollection<Playlist> Playlists
        {
            get
            {
                return new ObservableCollection<Playlist>(_playlistService.Playlists);
            }
        }

        private void UpdateAudioFiles(object sender, FilesChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => AudioFiles);
        }

        private void UpdatePlaylists(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => Playlists);
        }

        public MainViewModel(ILibraryService librarySerivce, IPlaylistService playlistService)
        {
            _libraryService = librarySerivce;
            _playlistService = playlistService;

            _libraryService.OnFilesChanged += UpdateAudioFiles;
            _libraryService.OnFinishedProcessing += OnFinishedProcessing;
        }

        private bool _validDragDrop;
        public void FilesDroppedOnWindow(DragEventArgs e)
        {
            if (!_validDragDrop) return;

            var fileNames = e.Data.GetDropFileNames();
            Task.Run(() => _libraryService.ProcessDragDrop(fileNames));
            NotifyOfPropertyChange(() => IsProcessing);
        }

        private bool ValidateDragDrop(string[] paths)
        {
            return paths.Any(x => x.IsAudioFile()) || paths.Any(x => x.IsDirectory());
        }

        public void FilesDraggedOnWindow(DragEventArgs e)
        {
            _validDragDrop = false;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = e.Data.GetDropFileNames();

            if (ValidateDragDrop(files))
            {
                e.Effects = DragDropEffects.Copy;
                _validDragDrop = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        public bool CanAddFiles { get { return !_libraryService.IsProcessing; } }
        public void AddFiles()
        {
            var files = DialogFactory.NewOpenFilesDialog(DialogFactory.AudioFilesFilter);
            if (files == null) return;

            _libraryService.ProcessDragDrop(files);
        }

        public bool CanAddFolder { get { return !_libraryService.IsProcessing; } }

        public void AddFolder()
        {
            var folder = DialogFactory.NewOpenFolderDialog();
            if (folder == null) return;

            _libraryService.ProcessDragDrop(new [] {folder});
        }

        public bool IsProcessing { get { return _libraryService.IsProcessing; } }

        private void OnFinishedProcessing(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(() => IsProcessing);
        }
    }
}

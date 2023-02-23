﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HashCalculator;

internal class AppViewModel : INotifyPropertyChanged
{
    private readonly ModelStarter starter = new(8);
    private static readonly Dispatcher AppDispatcher
        = Application.Current.Dispatcher;
    private static readonly object serialNumberLock = new();
    private delegate void ModelToTableDelegate(ModelArg arg);
    private readonly ModelToTableDelegate modelToTable;
    private int currentSerialNumber = 0;
    private int finishedNumberInQueue = 0;
    private int totalNumberInQueue = 0;
    private CancellationTokenSource? cancellation;
    private List<ModelArg> droppedFiles = new();
    private string hashCheckReport = string.Empty;
    private bool noExportColumn;
    private bool noDurationColumn;
    private QueueState queueState = QueueState.None;
    private static readonly object changeQueueCountLock = new();
    private static readonly object concurrentLock = new();
    private static readonly object displayModelLock = new();
    private static readonly object displayModelTaskLock = new();

    public static AppViewModel? Instance;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<HashViewModel> HashViewModels { get; }
        = new ObservableCollection<HashViewModel>();

    public bool NoDurationColumn
    {
        get { return this.noDurationColumn; }
        set { this.noDurationColumn = value; this.OnPropertyChanged(); }
    }

    public bool NoExportColumn
    {
        get { return this.noExportColumn; }
        set { this.noExportColumn = value; this.OnPropertyChanged(); }
    }

    public string Report
    {
        set
        {
            this.hashCheckReport = value;
            this.OnPropertyChanged();
        }
        get
        {
            if (this.hashCheckReport == string.Empty)
                return "暂无校验报告...";
            else
                return this.hashCheckReport;
        }
    }

    public QueueState State
    {
        get => this.queueState;
        set
        {
            if ((this.queueState == QueueState.None
                || this.queueState == QueueState.Stopped)
                && value == QueueState.Started)
            {
                AppDispatcher.Invoke(() => { this.Report = string.Empty; });
                this.queueState = value;
                this.OnPropertyChanged();
            }
            else if (this.queueState == QueueState.Started && value == QueueState.Stopped)
            {
                this.GenerateVerificationReport();
                this.queueState = value;
                this.OnPropertyChanged();
            }
        }
    }

    public int TotalNumberInQueue
    {
        get { return this.totalNumberInQueue; }
        set { this.totalNumberInQueue = value; this.OnPropertyChanged(); }
    }

    public int FinishedInQueue
    {
        get { return this.finishedNumberInQueue; }
        set { this.finishedNumberInQueue = value; this.OnPropertyChanged(); }
    }

    public AppViewModel()
    {
        this.modelToTable = new ModelToTableDelegate(this.ModelToTable);
        Instance = this;
    }

    private void ModelToTable(ModelArg arg)
    {
        int modelSerial = this.SerialGet();
        HashViewModel model = new(modelSerial, arg);
        model.ComputeFinishedEvent += this.IncreaseQueueFinished;
        model.WaitingModelCanceledEvent += this.DecreaseQueueTotal;
        model.ModelCanbeStartedEvent += this.starter.PendingModel;
        model.StartupModel(false);
        this.HashViewModels.Add(model);
    }

    private void QueueItemCountChanged()
    {
#if DEBUG
        Console.WriteLine(
            $"已完成任务：{this.FinishedInQueue}，"
            + $"总数：{this.TotalNumberInQueue}");
#endif
        if (this.FinishedInQueue != this.TotalNumberInQueue
            && this.State != QueueState.Started)
        {
            AppDispatcher.Invoke(() =>
            {
                this.State = QueueState.Started;
            });
        }
        else if (this.FinishedInQueue == this.TotalNumberInQueue
            && this.State != QueueState.Stopped)
        {
            AppDispatcher.Invoke(() =>
            {
                this.FinishedInQueue = this.TotalNumberInQueue = 0;
                this.State = QueueState.Stopped;
            });
        }
    }

    private void DisplayModels(IEnumerable<ModelArg> args, CancellationToken token)
    {
        int argsCount, remainingArgsCount;
        argsCount = remainingArgsCount = args.Count();
        if (argsCount == 0) return;
        AppDispatcher.Invoke(() => { this.IncreaseQueueTotal(argsCount); });
        lock (displayModelLock)
        {
            foreach (ModelArg arg in args)
            {
                if (token.IsCancellationRequested)
                {
                    AppDispatcher.Invoke(
                        () => { this.DecreaseQueueTotal(remainingArgsCount); });
                    break;
                }
                --remainingArgsCount;
                AppDispatcher.Invoke(this.modelToTable, arg);
                Thread.Sleep(1);
            }
        }
#if DEBUG
        Console.WriteLine($"已添加哈希模型：{argsCount - remainingArgsCount}");
#endif
    }

    private void DecreaseQueueTotal(int number)
    {
        lock (changeQueueCountLock)
        {
            if (number < 0) number = 0;
            this.TotalNumberInQueue -= number;
            this.QueueItemCountChanged();
        }
    }

    private void IncreaseQueueTotal(int number)
    {
        lock (changeQueueCountLock)
        {
            if (number < 0) number = 0;
            this.TotalNumberInQueue += number;
            this.QueueItemCountChanged();
        }
    }

    private void IncreaseQueueFinished(int number)
    {
        lock (changeQueueCountLock)
        {
            if (number < 0) number = 0;
            this.FinishedInQueue += number;
            this.QueueItemCountChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private int SerialGet()
    {
        lock (serialNumberLock) { return ++this.currentSerialNumber; }
    }

    private void SerialReset()
    {
        lock (serialNumberLock) { this.currentSerialNumber = 0; }
    }

    public void SetConcurrent(SimCalc num)
    {
        lock (concurrentLock)
        {
            switch (num)
            {
                case SimCalc.One:
                    this.starter.Adjust(1);
                    break;
                case SimCalc.Two:
                    this.starter.Adjust(2);
                    break;
                case SimCalc.Four:
                    this.starter.Adjust(4);
                    break;
                case SimCalc.Eight:
                    this.starter.Adjust(8);
                    break;
            }
        }
    }

    public void ClearHashViewModels()
    {
        this.droppedFiles.Clear();
        this.SerialReset();
        this.HashViewModels.Clear();
    }

    public void DisplayHashViewModelsTask(IEnumerable<ModelArg> args)
    {
        lock (displayModelTaskLock)
        {
            this.droppedFiles.AddRange(args);
            this.cancellation ??= GlobalCancellation.Handle;
            CancellationToken token = this.cancellation.Token;
            Task.Run(() => { this.DisplayModels(args, token); }, token);
        }
    }

    public void GenerateVerificationReport()
    {
        int noresult, unrelated, matched, mismatch,
            uncertain, succeeded, canceled, hasFailed;
        noresult = unrelated = matched = mismatch =
            uncertain = succeeded = canceled = hasFailed = 0;
        foreach (HashViewModel hm in this.HashViewModels)
        {
            switch (hm.CmpResult)
            {
                case CmpRes.NoResult:
                    ++noresult;
                    break;
                case CmpRes.Unrelated:
                    ++unrelated;
                    break;
                case CmpRes.Matched:
                    ++matched;
                    break;
                case CmpRes.Mismatch:
                    ++mismatch;
                    break;
                case CmpRes.Uncertain:
                    ++uncertain;
                    break;
            }
            switch (hm.Result)
            {
                case HashResult.Succeeded:
                    ++succeeded;
                    break;
                case HashResult.Canceled:
                    ++canceled;
                    break;
                case HashResult.HasFailed:
                    ++hasFailed;
                    break;
            }
        }
        this.Report
            = $"校验报告：\n\n已匹配：{matched}\n"
            + $"不匹配：{mismatch}\n"
            + $"不确定：{uncertain}\n"
            + $"无关联：{unrelated}\n"
            + $"未校验：{noresult} \n\n"
            + $"队列总数：{this.HashViewModels.Count}\n"
            + $"已成功：{succeeded}\n"
            + $"已失败：{hasFailed}\n"
            + $"已取消：{canceled}";
    }

    public void Models_CancelAll()
    {
        lock (displayModelTaskLock)
        {
            this.cancellation?.Cancel();
            foreach (var model in this.HashViewModels)
                model.ShutdownModel();
            this.cancellation?.Dispose();
            this.cancellation = GlobalCancellation.Handle;
        }
    }

    public static void Models_CancelOne(HashViewModel model)
    {
        model.ShutdownModel();
    }

    public void Models_ContinueAll()
    {
        foreach (var model in this.HashViewModels)
            model.PauseOrContinueModel(PauseMode.Continue);
    }

    public void Models_PauseAll()
    {
        foreach (var model in this.HashViewModels)
            model.PauseOrContinueModel(PauseMode.Pause);
    }

    public static void Models_PauseOne(HashViewModel model)
    {
        model.PauseOrContinueModel(PauseMode.Invert);
    }

    public void Models_Restart(bool newLines)
    {
        if (!newLines)
        {
            bool force = !Settings.Current.RecalculateIncomplete;
            int canbeStartModelCount = 0;
            foreach (var model in this.HashViewModels)
            {
                if (model.StartupModel(force))
                    ++canbeStartModelCount;
            }
            this.IncreaseQueueTotal(canbeStartModelCount);
        }
        else
        {
            if (this.droppedFiles.Count <= 0)
                return;
            List<ModelArg> args = this.droppedFiles;
            this.droppedFiles = new List<ModelArg>();
            this.DisplayHashViewModelsTask(args);
        }
    }

    public void Models_StartOne(HashViewModel viewModel)
    {
        this.IncreaseQueueTotal(1);
        viewModel.StartupModel(true);
    }

    public void RefreshCmpResultDisplayStyle()
    {
        foreach (var model in this.HashViewModels)
            model.CmpResult = model.CmpResult;
    }

    public void SetColumnVisibility(bool noExportColumn, bool noDurationColumn)
    {
        this.NoExportColumn = noExportColumn;
        this.NoDurationColumn = noDurationColumn;
    }
}

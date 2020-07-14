using RefactorIssue;
using RefactorIssue.SubObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//todo add Conflicts: Double versions of file, version missing of file, ...
namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileDisassemblerWorker
	{
		private readonly int _amountOfConcurrentJobs;
		private readonly WorkerQueueContainer _workerQueueContainer;

		public FileDisassemblerWorker(WorkerQueueContainer workerQueueContainer)
		{
			_amountOfConcurrentJobs = 10;
			_workerQueueContainer = workerQueueContainer;
		}

		protected async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				WorkerLog.Instance.Information($"Starting {nameof(FileDisassemblerWorker)}");
				var incomingQueue = _workerQueueContainer.ToProcessFiles.Reader;
				var outgoingQueue = _workerQueueContainer.ToSendChunks.Writer;
				var taskRunner = new TaskRunner(_amountOfConcurrentJobs);
				while (await incomingQueue.WaitToReadAsync() && !stoppingToken.IsCancellationRequested)
				{
					var file = await incomingQueue.ReadAsync(stoppingToken);
					if (System.IO.File.Exists(PathBuilder.BuildPath(file.Path)))
					{
						taskRunner.Add(async () =>
						{
							//START OF SECTION TO REFACTOR TO METHOD
							WorkerLog.Instance.Information($"'{file.Path}' is being processed");
							var recipients = await GetRecipients(file);

							if (recipients.Any())
							{
								var eofMessage = await SplitFile(file, recipients, outgoingQueue);
								if (eofMessage is object)
									await FinializeFileProcess(eofMessage, recipients);

								WorkerLog.Instance.Information($"'{file.Path}' is processed");
							}
							//END OF SECTION
						});
					}
					else
					{
						WorkerLog.Instance.Information($"File '{file.Path}' does not exist");
					}
				}
			});

			th.Start();

			await Task.Delay(Timeout.Infinite);
		}

		private Task<List<Recipient>> GetRecipients(File file)
		{
			return Task.FromResult<List<Recipient>>(null);
		}

		private async Task<object> SplitFile(File file, ICollection<Recipient> recipients, System.Threading.Channels.ChannelWriter<(Recipient recipient, FileChunk filechunk)> writer)
		{
			async Task ChunkCreatedCallBack(FileChunk fileChunk)
			{
				foreach (var recipient in recipients)
				{
					if (await writer.WaitToWriteAsync())
						await writer.WriteAsync((recipient, fileChunk));
				}
			}

			//return await _fileProcessService.ProcessFile(file, ChunkCreatedCallBack);
			return null;
		}

		private Task FinializeFileProcess(object eofMessage, ICollection<Recipient> recipients)
		{
			return Task.CompletedTask;
		}


	}
}

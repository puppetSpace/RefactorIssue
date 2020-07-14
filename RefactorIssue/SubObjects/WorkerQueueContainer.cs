using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RefactorIssue.SubObjects
{
	public class WorkerQueueContainer
	{
		public WorkerQueueContainer()
		{
			ToProcessFiles = Channel.CreateBounded<File>(new BoundedChannelOptions(50) { FullMode = BoundedChannelFullMode.Wait });
			ToSendFiles = Channel.CreateBounded<File>(new BoundedChannelOptions(50) { FullMode = BoundedChannelFullMode.Wait });
			ToSendChunks = Channel.CreateBounded<(Recipient recipient, FileChunk filechunk)>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait });
		}

		public Channel<File> ToProcessFiles { get; set; }

		public Channel<File> ToSendFiles { get; set; }

		public Channel<(Recipient recipient, FileChunk filechunk)> ToSendChunks { get; set; }
	}

	public class File
	{
		public Guid Id { get; set; }

		public Guid FolderId { get; set; }

		public string Name { get; set; }

		public int Version { get; set; }

		public long Size { get; set; }

		public DateTime LastModifiedDate { get; set; }

		public string Path { get; set; }

		public bool IsNew() => Version == 1;
	}

	public class FileChunk
	{
		public Guid Id { get; private set; }

		public Guid FileId { get; private set; }

		public int SequenceNo { get; private set; }

		public byte[] Value { get; private set; }
	}

	public class Recipient
	{
		public Guid Id { get; private set; }

		public string Name { get; private set; }

		public string Address { get; private set; }

		public bool Verified { get; private set; }
	}
}

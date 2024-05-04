using System;

namespace Ionic.Zlib;

internal sealed class InflateBlocks
{
	private enum InflateBlockMode
	{
		TYPE,
		LENS,
		STORED,
		TABLE,
		BTREE,
		DTREE,
		CODES,
		DRY,
		DONE,
		BAD
	}

	private const int MANY = 1440;

	internal static readonly int[] border = new int[19]
	{
		16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
		11, 4, 12, 3, 13, 2, 14, 1, 15
	};

	private InflateBlockMode mode;

	internal int left;

	internal int table;

	internal int index;

	internal int[] blens;

	internal int[] bb = new int[1];

	internal int[] tb = new int[1];

	internal InflateCodes codes = new InflateCodes();

	internal int last;

	internal ZlibCodec _codec;

	internal int bitk;

	internal int bitb;

	internal int[] hufts;

	internal byte[] window;

	internal int end;

	internal int readAt;

	internal int writeAt;

	internal object checkfn;

	internal uint check;

	internal InfTree inftree = new InfTree();

	internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
	{
		_codec = codec;
		hufts = new int[4320];
		window = new byte[w];
		end = w;
		this.checkfn = checkfn;
		mode = InflateBlockMode.TYPE;
		Reset();
	}

	internal uint Reset()
	{
		uint result = check;
		mode = InflateBlockMode.TYPE;
		bitk = 0;
		bitb = 0;
		readAt = (writeAt = 0);
		if (checkfn != null)
		{
			_codec._Adler32 = (check = Adler.Adler32(0u, null, 0, 0));
		}
		return result;
	}

	internal int Process(int r)
	{
		int p = _codec.NextIn;
		int n = _codec.AvailableBytesIn;
		int b = bitb;
		int k = bitk;
		int q = writeAt;
		int m = ((q < readAt) ? (readAt - q - 1) : (end - q));
		while (true)
		{
			switch (mode)
			{
			case InflateBlockMode.TYPE:
			{
				for (; k < 3; k += 8)
				{
					if (n != 0)
					{
						r = 0;
						n--;
						b |= (_codec.InputBuffer[p++] & 0xFF) << k;
						continue;
					}
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				int t = b & 7;
				last = t & 1;
				switch ((uint)(t >>> 1))
				{
				case 0u:
					b >>= 3;
					k -= 3;
					t = k & 7;
					b >>= t;
					k -= t;
					mode = InflateBlockMode.LENS;
					break;
				case 1u:
				{
					int[] bl = new int[1];
					int[] bd = new int[1];
					int[][] tl = new int[1][];
					int[][] td = new int[1][];
					InfTree.inflate_trees_fixed(bl, bd, tl, td, _codec);
					codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
					b >>= 3;
					k -= 3;
					mode = InflateBlockMode.CODES;
					break;
				}
				case 2u:
					b >>= 3;
					k -= 3;
					mode = InflateBlockMode.TABLE;
					break;
				case 3u:
					b >>= 3;
					k -= 3;
					mode = InflateBlockMode.BAD;
					_codec.Message = "invalid block type";
					r = -3;
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				break;
			}
			case InflateBlockMode.LENS:
				for (; k < 32; k += 8)
				{
					if (n != 0)
					{
						r = 0;
						n--;
						b |= (_codec.InputBuffer[p++] & 0xFF) << k;
						continue;
					}
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				if (((~b >> 16) & 0xFFFF) != (b & 0xFFFF))
				{
					mode = InflateBlockMode.BAD;
					_codec.Message = "invalid stored block lengths";
					r = -3;
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				left = b & 0xFFFF;
				b = (k = 0);
				mode = ((left != 0) ? InflateBlockMode.STORED : ((last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE));
				break;
			case InflateBlockMode.STORED:
			{
				if (n == 0)
				{
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				if (m == 0)
				{
					if (q == end && readAt != 0)
					{
						q = 0;
						m = ((q < readAt) ? (readAt - q - 1) : (end - q));
					}
					if (m == 0)
					{
						writeAt = q;
						r = Flush(r);
						q = writeAt;
						m = ((q < readAt) ? (readAt - q - 1) : (end - q));
						if (q == end && readAt != 0)
						{
							q = 0;
							m = ((q < readAt) ? (readAt - q - 1) : (end - q));
						}
						if (m == 0)
						{
							bitb = b;
							bitk = k;
							_codec.AvailableBytesIn = n;
							_codec.TotalBytesIn += p - _codec.NextIn;
							_codec.NextIn = p;
							writeAt = q;
							return Flush(r);
						}
					}
				}
				r = 0;
				int t = left;
				if (t > n)
				{
					t = n;
				}
				if (t > m)
				{
					t = m;
				}
				Array.Copy(_codec.InputBuffer, p, window, q, t);
				p += t;
				n -= t;
				q += t;
				m -= t;
				if ((left -= t) == 0)
				{
					mode = ((last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
				}
				break;
			}
			case InflateBlockMode.TABLE:
			{
				for (; k < 14; k += 8)
				{
					if (n != 0)
					{
						r = 0;
						n--;
						b |= (_codec.InputBuffer[p++] & 0xFF) << k;
						continue;
					}
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				int t = (table = b & 0x3FFF);
				if ((t & 0x1F) > 29 || ((t >> 5) & 0x1F) > 29)
				{
					mode = InflateBlockMode.BAD;
					_codec.Message = "too many length or distance symbols";
					r = -3;
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				t = 258 + (t & 0x1F) + ((t >> 5) & 0x1F);
				if (blens == null || blens.Length < t)
				{
					blens = new int[t];
				}
				else
				{
					Array.Clear(blens, 0, t);
				}
				b >>= 14;
				k -= 14;
				index = 0;
				mode = InflateBlockMode.BTREE;
				goto case InflateBlockMode.BTREE;
			}
			case InflateBlockMode.BTREE:
			{
				while (index < 4 + (table >> 10))
				{
					for (; k < 3; k += 8)
					{
						if (n != 0)
						{
							r = 0;
							n--;
							b |= (_codec.InputBuffer[p++] & 0xFF) << k;
							continue;
						}
						bitb = b;
						bitk = k;
						_codec.AvailableBytesIn = n;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					blens[border[index++]] = b & 7;
					b >>= 3;
					k -= 3;
				}
				while (index < 19)
				{
					blens[border[index++]] = 0;
				}
				bb[0] = 7;
				int t = inftree.inflate_trees_bits(blens, bb, tb, hufts, _codec);
				if (t != 0)
				{
					r = t;
					if (r == -3)
					{
						blens = null;
						mode = InflateBlockMode.BAD;
					}
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				index = 0;
				mode = InflateBlockMode.DTREE;
				goto case InflateBlockMode.DTREE;
			}
			case InflateBlockMode.DTREE:
			{
				int t;
				while (true)
				{
					t = table;
					if (index >= 258 + (t & 0x1F) + ((t >> 5) & 0x1F))
					{
						break;
					}
					for (t = bb[0]; k < t; k += 8)
					{
						if (n != 0)
						{
							r = 0;
							n--;
							b |= (_codec.InputBuffer[p++] & 0xFF) << k;
							continue;
						}
						bitb = b;
						bitk = k;
						_codec.AvailableBytesIn = n;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					t = hufts[(tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 1];
					int c = hufts[(tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 2];
					if (c < 16)
					{
						b >>= t;
						k -= t;
						blens[index++] = c;
						continue;
					}
					int i = ((c == 18) ? 7 : (c - 14));
					int j = ((c == 18) ? 11 : 3);
					for (; k < t + i; k += 8)
					{
						if (n != 0)
						{
							r = 0;
							n--;
							b |= (_codec.InputBuffer[p++] & 0xFF) << k;
							continue;
						}
						bitb = b;
						bitk = k;
						_codec.AvailableBytesIn = n;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					b >>= t;
					k -= t;
					j += b & InternalInflateConstants.InflateMask[i];
					b >>= i;
					k -= i;
					i = index;
					t = table;
					if (i + j > 258 + (t & 0x1F) + ((t >> 5) & 0x1F) || (c == 16 && i < 1))
					{
						blens = null;
						mode = InflateBlockMode.BAD;
						_codec.Message = "invalid bit length repeat";
						r = -3;
						bitb = b;
						bitk = k;
						_codec.AvailableBytesIn = n;
						_codec.TotalBytesIn += p - _codec.NextIn;
						_codec.NextIn = p;
						writeAt = q;
						return Flush(r);
					}
					c = ((c == 16) ? blens[i - 1] : 0);
					do
					{
						blens[i++] = c;
					}
					while (--j != 0);
					index = i;
				}
				tb[0] = -1;
				int[] bl = new int[1] { 9 };
				int[] bd = new int[1] { 6 };
				int[] tl = new int[1];
				int[] td = new int[1];
				t = table;
				t = inftree.inflate_trees_dynamic(257 + (t & 0x1F), 1 + ((t >> 5) & 0x1F), blens, bl, bd, tl, td, hufts, _codec);
				if (t != 0)
				{
					if (t == -3)
					{
						blens = null;
						mode = InflateBlockMode.BAD;
					}
					r = t;
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				codes.Init(bl[0], bd[0], hufts, tl[0], hufts, td[0]);
				mode = InflateBlockMode.CODES;
				goto case InflateBlockMode.CODES;
			}
			case InflateBlockMode.CODES:
				bitb = b;
				bitk = k;
				_codec.AvailableBytesIn = n;
				_codec.TotalBytesIn += p - _codec.NextIn;
				_codec.NextIn = p;
				writeAt = q;
				r = codes.Process(this, r);
				if (r != 1)
				{
					return Flush(r);
				}
				r = 0;
				p = _codec.NextIn;
				n = _codec.AvailableBytesIn;
				b = bitb;
				k = bitk;
				q = writeAt;
				m = ((q < readAt) ? (readAt - q - 1) : (end - q));
				if (last == 0)
				{
					mode = InflateBlockMode.TYPE;
					break;
				}
				mode = InflateBlockMode.DRY;
				goto case InflateBlockMode.DRY;
			case InflateBlockMode.DRY:
				writeAt = q;
				r = Flush(r);
				q = writeAt;
				m = ((q < readAt) ? (readAt - q - 1) : (end - q));
				if (readAt != writeAt)
				{
					bitb = b;
					bitk = k;
					_codec.AvailableBytesIn = n;
					_codec.TotalBytesIn += p - _codec.NextIn;
					_codec.NextIn = p;
					writeAt = q;
					return Flush(r);
				}
				mode = InflateBlockMode.DONE;
				goto case InflateBlockMode.DONE;
			case InflateBlockMode.DONE:
				r = 1;
				bitb = b;
				bitk = k;
				_codec.AvailableBytesIn = n;
				_codec.TotalBytesIn += p - _codec.NextIn;
				_codec.NextIn = p;
				writeAt = q;
				return Flush(r);
			case InflateBlockMode.BAD:
				r = -3;
				bitb = b;
				bitk = k;
				_codec.AvailableBytesIn = n;
				_codec.TotalBytesIn += p - _codec.NextIn;
				_codec.NextIn = p;
				writeAt = q;
				return Flush(r);
			default:
				r = -2;
				bitb = b;
				bitk = k;
				_codec.AvailableBytesIn = n;
				_codec.TotalBytesIn += p - _codec.NextIn;
				_codec.NextIn = p;
				writeAt = q;
				return Flush(r);
			}
		}
	}

	internal void Free()
	{
		Reset();
		window = null;
		hufts = null;
	}

	internal void SetDictionary(byte[] d, int start, int n)
	{
		Array.Copy(d, start, window, 0, n);
		readAt = (writeAt = n);
	}

	internal int SyncPoint()
	{
		if (mode != InflateBlockMode.LENS)
		{
			return 0;
		}
		return 1;
	}

	internal int Flush(int r)
	{
		for (int pass = 0; pass < 2; pass++)
		{
			int nBytes = ((pass != 0) ? (writeAt - readAt) : (((readAt <= writeAt) ? writeAt : end) - readAt));
			if (nBytes == 0)
			{
				if (r == -5)
				{
					r = 0;
				}
				return r;
			}
			if (nBytes > _codec.AvailableBytesOut)
			{
				nBytes = _codec.AvailableBytesOut;
			}
			if (nBytes != 0 && r == -5)
			{
				r = 0;
			}
			_codec.AvailableBytesOut -= nBytes;
			_codec.TotalBytesOut += nBytes;
			if (checkfn != null)
			{
				_codec._Adler32 = (check = Adler.Adler32(check, window, readAt, nBytes));
			}
			Array.Copy(window, readAt, _codec.OutputBuffer, _codec.NextOut, nBytes);
			_codec.NextOut += nBytes;
			readAt += nBytes;
			if (readAt == end && pass == 0)
			{
				readAt = 0;
				if (writeAt == end)
				{
					writeAt = 0;
				}
			}
			else
			{
				pass++;
			}
		}
		return r;
	}
}

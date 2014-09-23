using System.Collections;
using System.IO;
using System;

public class SpPacker {
    private static int PackSeg (byte[] src, int src_offset, byte[] dest, int dest_offset, int dest_size, int n) {
        byte header = 0;
        int notzero = 0;

        int dest_begin = dest_offset;
        dest_offset++;
        if (dest_offset >= dest_size)
            dest_begin = -1;

        for (int i = 0; i < 8; i++) {
            if (src[src_offset + i] != 0) {
                notzero++;
                header |= (byte)(1 << i);

                if (dest_offset < dest_size) {
                    dest[dest_offset] = src[src_offset + i];
                    dest_offset++;
                }
            }
        }

        if ((notzero == 7 || notzero == 6) && n > 0)
            notzero = 8;

        if (notzero == 8) {
            if (n > 0)
                return 8;
            else
                return 10;
        }

        if (dest_begin >= 0)
            dest[dest_begin] = header;

        return notzero + 1;
    }

    public static void WriteFF (byte[] src, int src_offset, byte[] dest, int dest_offset, int n) {
        dest[dest_offset] = 0xff;
        dest[dest_offset + 1] = (byte)(n - 1);
        Array.Copy (src, src_offset, dest, dest_offset + 2, n * 8);
    }

    public static bool Pack (Stream input, Stream output) {
        int size = 0;

        int src_size = (int)(((input.Length + 7) / 8) * 8);
        byte[] src = new byte[src_size];
        input.Read (src, 0, (int)input.Length);
        int dest_size = (src_size + 2047) / 2048 * 2 + src_size;
        byte[] dest = new byte[dest_size];

        int dest_offset = 0;
        int ff_n = 0;
        int ff_src_start = 0;
        int ff_dest_start = 0;

        for (int src_offset = 0; src_offset < src_size; src_offset += 8) {
            int n = PackSeg (src, src_offset, dest, dest_offset, dest_size, ff_n);

            if (n == 10) {
                ff_src_start = src_offset;
                ff_dest_start = dest_offset;
                ff_n = 1;
            }
            else if (n == 8 && ff_n > 0) {
                ff_n++;
                if (ff_n == 256) {
                    if (dest_offset < dest_size) {
                        WriteFF (src, ff_src_start, dest, ff_dest_start, 256);
                    }
                    ff_n = 0;
                }
            }
            else {
                if (ff_n > 0) {
                    if (dest_offset < dest_size) {
                        WriteFF (src, ff_src_start, dest, ff_dest_start, ff_n);
                    }
                    ff_n = 0;
                }
            }

            dest_offset += n;
            size += n;
        }

        if (ff_n > 0 && dest_offset < dest_size)
            WriteFF (src, ff_src_start, dest, ff_dest_start, ff_n);

        output.Write (dest, 0, size);
        return (size <= dest_size);
    }

    public static bool Unpack (Stream input, Stream output) {
		int size = 0;
		
		int src_size = (int)input.Length;
		byte[] src = new byte[src_size];
		input.Read (src, 0, src_size);
		int dest_size = src_size ;
		byte[] dest = new byte[dest_size];

		int src_offset = 0;
		int dest_offset = 0;
		while (src_offset < src_size) {
			byte header = src[src_offset];
			src_offset++;

			if (header == 0xff) {
				if (src_offset >= src_size)
					return false;

				int n = (src[src_offset] + 1) * 8;
				if (src_size - src_offset < n + 1)
					return false;

				if (dest_size < n) {
					output.Write (dest, 0, dest_offset);
					dest = new byte[n + 1];
					dest_offset = 0;
				}

				if (dest_size - dest_offset < n + 1) {
					output.Write (dest, 0, dest_offset);
					dest_offset = 0;
				}

				src_offset++;
				Array.Copy (src, src_offset, dest, dest_offset, n);
				src_offset += n;
				dest_offset += n;
				size += n;

			}
			else {
				for (int i = 0; i < 8; i++) {
					int nz = (header >> i) & 1;
					if (nz != 0) {
						if (src_offset >= src_size)
							return false;

						if (dest_offset < dest_size) {
							dest[dest_offset] = src[src_offset];
							dest_offset++;
						}
						src_offset++;
					}
					else {
						if (dest_offset < dest_size) {
							dest[dest_offset] = 0;
							dest_offset++;
						}
					}

					if (dest_size == dest_offset) {
						output.Write (dest, 0, dest_offset);
						dest_offset = 0;
					}
					size++;
				}
			}
		}
		
		output.Write (dest, 0, dest_offset);
		return true;
    }
}

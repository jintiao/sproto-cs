using System.Collections.Generic;
using System;

public class SpStream {
    private byte[] mBuffer;
    private int mOffset;
    private int mLength;
    private int mPosition;
    private int mTail;

    public SpStream ()
        : this (64) {
    }

    public SpStream (int len)
        : this (new byte[len]) {
    }

    public SpStream (byte[] b) 
        : this (b, 0, b.Length) {
    }

    public SpStream (byte[] b, int o, int c) {
        mBuffer = b;
        mLength = o + c;
        mOffset = o;
        mPosition = mOffset;
        mTail = mPosition;
    }

    public bool IsOverflow () {
        return (mPosition > mLength);
    }

    public byte ReadByte () {
        int pos = mPosition;
        mPosition += 1;
        return mBuffer[pos];
    }

    public bool ReadBoolean () {
        return (ReadByte () != 0);
    }

    public short ReadInt16 () {
        int pos = mPosition;
        mPosition += 2;
        return BitConverter.ToInt16 (mBuffer, pos);
    }

    public ushort ReadUInt16 () {
        int pos = mPosition;
        mPosition += 2;
        return BitConverter.ToUInt16 (mBuffer, pos);
    }

    public int ReadInt32 () {
        int pos = mPosition;
        mPosition += 4;
        return BitConverter.ToInt32 (mBuffer, pos);
    }

    public long ReadInt64 () {
        int pos = mPosition;
        mPosition += 8;
        return BitConverter.ToInt64 (mBuffer, pos);
    }

    public byte[] ReadBytes (int len) {
        byte[] bytes = new byte[len];
        for (int i = 0; i < len; i++) {
            bytes[i] = mBuffer[mPosition + i];
        }
        mPosition += len;
        return bytes;
    }

    public int Read (byte[] bytes) {
        return Read (bytes, 0, bytes.Length);
    }

    public int Read (byte[] bytes, int offset, int length) {
        for (int i = 0; i < length; i++) {
            if (mPosition >= mTail)
                return i;
            bytes[i + offset] = mBuffer[mPosition];
            mPosition++;
        }

        return length;
    }

    public void Write (short n) {
        Write (BitConverter.GetBytes (n));
    }

    public void Write (int n) {
        Write (BitConverter.GetBytes (n));
    }

    public void Write (long n) {
        Write (BitConverter.GetBytes (n));
    }

    public void Write (byte b) {
        if (mPosition < mLength) {
            mBuffer[mPosition] = b;
        }
        mPosition += 1;
        if (mPosition > mTail) {
            mTail = mPosition;
            if (mTail > mLength)
                mTail = mLength;
        }
    }

    public void Write (byte[] bytes) {
        Write (bytes, 0, bytes.Length);
    }

    public void Write (byte[] bytes, int offset, int length) {
        for (int i = 0; i < length; i++) {
            if (IsOverflow ())
                break;
            mBuffer[mPosition] = bytes[i + offset];
            mPosition++;
        }

        if (mPosition > mTail) {
            mTail = mPosition;
            if (mTail > mLength)
                mTail = mLength;
        }
    }

    public int Position { 
        get { return mPosition; }
        set { mPosition = value; }
    }

    public int Length { get { return mLength; } }
}

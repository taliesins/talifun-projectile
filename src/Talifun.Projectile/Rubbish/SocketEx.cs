﻿/*
 * The MIT License (MIT)
Copyright © 2013 Şafak Gür
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 */

using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace Talifun.Projectile.Rubbish
{
    /// <summary>
    ///     Provides socket extensions for easier asynchronous operations.
    /// </summary>
    public static class SocketEx
    {
        /// <summary>
        ///     Holds a delegate of <see cref="Socket" />'s accept operation.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Func<Socket, SocketAwaitable, bool> AcceptOp = (s, a) =>
            s.AcceptAsync(a.Arguments);

        /// <summary>
        ///     Holds a delegate of <see cref="Socket" />'s connect operation.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Func<Socket, SocketAwaitable, bool> ConnectOp = (s, a) =>
            s.ConnectAsync(a.Arguments);

        /// <summary>
        ///     Holds a delegate of <see cref="Socket" />'s disconnect operation.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Func<Socket, SocketAwaitable, bool> DisconnectOp = (s, a) =>
            s.DisconnectAsync(a.Arguments);

        /// <summary>
        ///     Holds a delegate of <see cref="Socket" />'s receive operation.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Func<Socket, SocketAwaitable, bool> ReceiveOp = (s, a) =>
            s.ReceiveAsync(a.Arguments);

        /// <summary>
        ///     Holds a delegate of <see cref="Socket" />'s send operation.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Func<Socket, SocketAwaitable, bool> SendOp = (s, a) =>
            s.SendAsync(a.Arguments);

        /// <summary>
        ///     Begins an awaitable operation to accept an incoming connection attempt.
        /// </summary>
        /// <param name="socket">
        ///     Socket that will accept the connection.
        /// </param>
        /// <param name="awaitable">
        ///     The <see cref="SocketAwaitable" /> object to use for this asynchronous socket
        ///     operation.
        /// </param>
        /// <returns>
        ///     <paramref name="awaitable" />, when awaited, will have the accepted socket in its
        ///     <see cref="SocketAwaitable.AcceptSocket" /> property. Awaiter of the result returns
        ///     a <see cref="SocketError" /> that corresponds to the result of this asynchronous
        ///     operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="socket" /> or <paramref name="awaitable" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <see cref="SocketAwaitable.Buffer" /> of the <paramref name="awaitable" /> is not
        ///     large enough. The buffer must be at least 2 * (sizeof(SOCKADDR_STORAGE + 16) bytes.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     <paramref name="socket" /> is not bound, is not listening for connections, or is
        ///     already connected.
        ///     -or-
        ///     A socket operation was already in progress using <paramref name="awaitable" />
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Windows XP or later is required for this method.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     <paramref name="socket" /> has been disposed.
        /// </exception>
        public static SocketAwaitable AcceptAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperateAsync(socket, awaitable, AcceptOp);
        }

        /// <summary>
        ///     Begins an awaitable request for a connection to a remote host.
        /// </summary>
        /// <param name="socket">
        ///     Socket that will connect to a remote host.
        /// </param>
        /// <param name="awaitable">
        ///     The <see cref="SocketAwaitable" /> object to use for this asynchronous socket
        ///     operation.
        /// </param>
        /// <returns>
        ///     The specified <see cref="SocketAwaitable" /> which, when awaited, returns a
        ///     <see cref="SocketError" /> object that corresponds to the result of the connection
        ///     attempt.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="socket" />, <paramref name="awaitable" />, or
        ///     <see cref="SocketAwaitable.RemoteEndPoint" /> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     <paramref name="socket" /> is listening or a socket operation was already in
        ///     progress using <paramref name="awaitable" />.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Windows XP or later is required for this method. This exception also occurs if the
        ///     local endpoint and the <see cref="SocketAwaitable.RemoteEndPoint" /> are not the
        ///     same address family.
        ///     -or-
        ///     Address family of <see cref="Socket.LocalEndPoint" /> is different than the address
        ///     family of <see cref="SocketAsyncEventArgs.RemoteEndPoint" />.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     <paramref name="socket" /> has been disposed.
        /// </exception>
        /// <exception cref="SecurityException">
        ///     A caller higher in the call stack does not have permission for the requested
        ///     operation.
        /// </exception>
        public static SocketAwaitable ConnectAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperateAsync(socket, awaitable, ConnectOp);
        }

        /// <summary>
        ///     Begins an awaitable request to disconnect from a remote endpoint.
        /// </summary>
        /// <param name="socket">
        ///     Socket that will connect to a remote host.
        /// </param>
        /// <param name="awaitable">
        ///     The <see cref="SocketAwaitable" /> object to use for this asynchronous socket
        ///     operation.
        /// </param>
        /// <returns>
        ///     The specified <see cref="SocketAwaitable" /> which, when awaited, returns a
        ///     <see cref="SocketError" /> object that corresponds to the result of the connection
        ///     attempt.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="socket" /> or <paramref name="awaitable" /> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     A socket operation was already in progress using <paramref name="awaitable" />.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Windows XP or later is required for this method. This exception also occurs if the
        ///     local endpoint and the <see cref="SocketAwaitable.RemoteEndPoint" /> are not the
        ///     same address family.
        ///     -or-
        ///     Address family of <see cref="Socket.LocalEndPoint" /> is different than the address
        ///     family of <see cref="SocketAsyncEventArgs.RemoteEndPoint" />.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     <paramref name="socket" /> has been disposed.
        /// </exception>
        public static SocketAwaitable DisonnectAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperateAsync(socket, awaitable, DisconnectOp);
        }

        /// <summary>
        ///     Begins an awaitable request to receive data from a connected <see cref="Socket" />
        ///     object.
        /// </summary>
        /// <param name="socket">
        ///     Socket that will receive data.
        /// </param>
        /// <param name="awaitable">
        ///     The <see cref="SocketAwaitable" /> object to use for this asynchronous socket
        ///     operation.
        /// </param>
        /// <returns>
        ///     The specified <see cref="SocketAwaitable" /> which, when awaited, will hold the
        ///     received data in its <see cref="SocketAsyncEventArgs.Buffer" /> property. Awaiter
        ///     of <see cref="SocketAwaitable" /> returns a <see cref="SocketError" /> object that
        ///     corresponds to the result of the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="socket" /> or <paramref name="awaitable" /> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     A socket operation was already in progress using <paramref name="awaitable"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Windows XP or later is required for this method.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     <paramref name="socket" /> has been disposed.
        /// </exception>
        public static SocketAwaitable ReceiveAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperateAsync(socket, awaitable, ReceiveOp);
        }

        /// <summary>
        ///     Sends data asynchronously to a connected <see cref="Socket" /> object and returns a
        ///     <see cref="SocketAwaitable" /> to await.
        /// </summary>
        /// <param name="socket">
        ///     Socket to send the data to.
        /// </param>
        /// <param name="awaitable">
        ///     The <see cref="SocketAwaitable" /> object to use for this asynchronous socket
        ///     operation.
        /// </param>
        /// <returns>
        ///     The specified <see cref="SocketAwaitable" /> which, when awaited, will return a
        ///     <see cref="SocketError" /> object that corresponds to the result of the send
        ///     operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="socket" /> or <paramref name="awaitable" /> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     A socket operation was already in progress using <paramref name="awaitable"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Windows XP or later is required for this method.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     <paramref name="socket" /> has been disposed.
        /// </exception>
        public static SocketAwaitable SendAsync(this Socket socket, SocketAwaitable awaitable)
        {
            return OperateAsync(socket, awaitable, SendOp);
        }

        /// <summary>
        ///     Calls the specified asynchronous method of a <see cref="Socket" /> and returns an
        ///     awaitable object that provides the operation result when awaited.
        /// </summary>
        /// <param name="socket">
        ///     <see cref="Socket" /> to run an asynchronous operation.
        /// </param>
        /// <param name="awaitable">
        ///     The <see cref="SocketAwaitable" /> object to use for this asynchronous socket
        ///     operation.
        /// </param>
        /// <param name="operation">
        ///     Socket operation to perform.
        /// </param>
        /// <returns>
        ///     A <see cref="SocketAwaitable" /> which, when awaited, returns a
        ///     <see cref="SocketError" /> object that corresponds to the result of
        ///     <paramref name="operation" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="socket" /> or <paramref name="awaitable" /> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     A socket operation was already in progress using <paramref name="awaitable"/>.
        ///     -or-
        ///     For accept operations:
        ///     <paramref name="socket" /> is not bound, is not listening for connections, or is
        ///     already connected.
        ///     -or-
        ///     For connect operations:
        ///     <paramref name="socket" /> is listening.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Windows XP or later is required for this method.
        ///     -or-
        ///     For connect operations:
        ///     Address family of <see cref="Socket.LocalEndPoint" /> is different than the address
        ///     family of <see cref="SocketAsyncEventArgs.RemoteEndPoint" />.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     <paramref name="socket" /> has been disposed.
        /// </exception>
        /// <exception cref="SecurityException">
        ///     For connection operations:
        ///     A caller higher in the call stack does not have permission for the requested
        ///     operation.
        /// </exception>
        private static SocketAwaitable OperateAsync(
            Socket socket,
            SocketAwaitable awaitable,
            Func<Socket, SocketAwaitable, bool> operation)
        {
            if (socket == null)
                throw new ArgumentNullException("socket", "Socket must not be null.");

            if (awaitable == null)
                throw new ArgumentNullException("awaitable", "Awaitable must not be null.");

            var a = awaitable.GetAwaiter();

            lock (a.SyncRoot)
            {
                if (!a.IsCompleted)
                {
                    throw new InvalidOperationException(
                        "A socket operation is already in progress"
                        + " using the same awaitable arguments.");
                }

                a.Reset();
                if (awaitable.ShouldCaptureContext)
                {
                    a.SyncContext = SynchronizationContext.Current;
                }
            }

            try
            {
                if (!operation.Invoke(socket, awaitable))
                {
                    a.Complete();
                }
            }
            catch (SocketException x)
            {
                a.Complete();
                awaitable.Arguments.SocketError = x.SocketErrorCode != SocketError.Success
                    ? x.SocketErrorCode
                    : SocketError.SocketError;
            }
            catch (Exception)
            {
                a.Complete();
                awaitable.Arguments.SocketError = SocketError.Success;
                throw;
            }

            return awaitable;
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Talifun.Projectile.Rubbish.Helper
{
    /// <summary>
    /// Listens for connections from UDT network clients.
    /// </summary>
    public class UdtListener
    {
        private IPEndPoint m_ServerSocketEP;
        private Socket m_ServerSocket;
        private bool m_Active;
        private bool m_ExclusiveAddressUse;

        private static bool ValidateUdpPort(int port)
        {
            if (port >= 0)
                return port <= (int)ushort.MaxValue;
            else
                return false;
        }

        /// <summary>
        /// Gets the underlying network <see cref="T:System.Net.Sockets.Socket"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The underlying <see cref="T:System.Net.Sockets.Socket"/>.
        /// </returns>
        public Socket Server
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] get { return this.m_ServerSocket; }
        }

        /// <summary>
        /// Gets a value that indicates whether <see cref="T:System.Net.Sockets.UdtListener"/> is actively listening for client connections.
        /// </summary>
        /// 
        /// <returns>
        /// true if <see cref="T:System.Net.Sockets.UdtListener"/> is actively listening; otherwise, false.
        /// </returns>
        protected bool Active
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] get { return this.m_Active; }
        }

        /// <summary>
        /// Gets the underlying <see cref="T:System.Net.EndPoint"/> of the current <see cref="T:System.Net.Sockets.UdtListener"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The <see cref="T:System.Net.EndPoint"/> to which the <see cref="T:System.Net.Sockets.Socket"/> is bound.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public EndPoint LocalEndpoint
        {
            get
            {
                if (!this.m_Active)
                    return (EndPoint) this.m_ServerSocketEP;
                else
                    return this.m_ServerSocket.LocalEndPoint;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="T:System.Boolean"/> value that specifies whether the <see cref="T:System.Net.Sockets.UdtListener"/> allows only one underlying socket to listen to a specific port.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:System.Net.Sockets.UdtListener"/> allows only one <see cref="T:System.Net.Sockets.UdtListener"/> to listen to a specific port; otherwise, false. . The default is true for Windows Server 2003 and Windows XP Service Pack 2 and later, and false for all other versions.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdtListener"/> has been started. Call the <see cref="M:System.Net.Sockets.UdtListener.Stop"/> method and then set the <see cref="P:System.Net.Sockets.Socket.ExclusiveAddressUse"/> property.</exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the underlying socket.</exception><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public bool ExclusiveAddressUse
        {
            get { return this.m_ServerSocket.ExclusiveAddressUse; }
            set
            {
                if (this.m_Active)
                    throw new InvalidOperationException("net_UdtListener_mustbestopped");
                this.m_ServerSocket.ExclusiveAddressUse = value;
                this.m_ExclusiveAddressUse = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.UdtListener"/> class with the specified local endpoint.
        /// </summary>
        /// <param name="localEP">An <see cref="T:System.Net.IPEndPoint"/> that represents the local endpoint to which to bind the listener <see cref="T:System.Net.Sockets.Socket"/>. </param><exception cref="T:System.ArgumentNullException"><paramref name="localEP"/> is null. </exception>
        public UdtListener(IPEndPoint localEP)
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "UdtListener", (object) localEP);
            if (localEP == null)
                throw new ArgumentNullException("localEP");
            this.m_ServerSocketEP = localEP;
            this.m_ServerSocket = new Socket(this.m_ServerSocketEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            //if (!Logging.On)
            //    return;
            //Logging.Exit(Logging.Sockets, (object) this, "UdtListener", (string) null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.UdtListener"/> class that listens for incoming connection attempts on the specified local IP address and port number.
        /// </summary>
        /// <param name="localaddr">An <see cref="T:System.Net.IPAddress"/> that represents the local IP address. </param><param name="port">The port on which to listen for incoming connection attempts. </param><exception cref="T:System.ArgumentNullException"><paramref name="localaddr"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is not between <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="F:System.Net.IPEndPoint.MaxPort"/>. </exception>
        public UdtListener(IPAddress localaddr, int port)
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "UdtListener", (object) localaddr);
            if (localaddr == null)
                throw new ArgumentNullException("localaddr");
            if (!ValidateUdpPort(port))
                throw new ArgumentOutOfRangeException("port");
            this.m_ServerSocketEP = new IPEndPoint(localaddr, port);
            this.m_ServerSocket = new Socket(this.m_ServerSocketEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            //if (!Logging.On)
            //    return;
            //Logging.Exit(Logging.Sockets, (object) this, "UdtListener", (string) null);
        }



        /// <summary>
        /// Creates a new <see cref="T:System.Net.Sockets.UdtListener"/> instance to listen on the specified port.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Net.Sockets.UdtListener"/>.A new <see cref="T:System.Net.Sockets.UdtListener"/> instance to listen on the specified port.
        /// </returns>
        /// <param name="port">The port on which to listen for incoming connection attempts.</param>
        public static UdtListener Create(int port)
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, "UdtListener.Create", "Port: " + (object) port);
            if (!ValidateUdpPort(port))
                throw new ArgumentOutOfRangeException("port");
            UdtListener UdtListener = new UdtListener(IPAddress.IPv6Any, port);
            UdtListener.Server.DualMode = true;
            //if (Logging.On)
            //    Logging.Exit(Logging.Sockets, "UdtListener.Create", "Port: " + (object) port);
            return UdtListener;
        }

        /// <summary>
        /// Enables or disables Network Address Translation (NAT) traversal on a <see cref="T:System.Net.Sockets.UdtListener"/> instance.
        /// </summary>
        /// <param name="allowed">A Boolean value that specifies whether to enable or disable NAT traversal.</param><exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.Sockets.UdtListener.AllowNatTraversal(System.Boolean)"/> method was called after calling the <see cref="M:System.Net.Sockets.UdtListener.Start"/> method</exception>
        public void AllowNatTraversal(bool allowed)
        {
            if (this.m_Active)
                throw new InvalidOperationException("net_UdtListener_mustbestopped");
            if (allowed)
                this.m_ServerSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            else
                this.m_ServerSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
        }

        /// <summary>
        /// Starts listening for incoming connection requests.
        /// </summary>
        /// <exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode"/> property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Start()
        {
            this.Start(int.MaxValue);
        }

        /// <summary>
        /// Starts listening for incoming connection requests with a maximum number of pending connection.
        /// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue.</param><exception cref="T:System.Net.Sockets.SocketException">An error occurred while accessing the socket. See the Remarks section for more information. </exception><exception cref="T:System.ArgumentOutOfRangeException">The<paramref name=" backlog"/> parameter is less than zero or exceeds the maximum number of permitted connections.</exception><exception cref="T:System.InvalidOperationException">The underlying <see cref="T:System.Net.Sockets.Socket"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void Start(int backlog)
        {
            if (backlog > int.MaxValue || backlog < 0)
                throw new ArgumentOutOfRangeException("backlog");
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "Start", (string) null);
            if (this.m_ServerSocket == null)
                throw new InvalidOperationException("net_InvalidSocketHandle");
            if (this.m_Active)
            {
                //if (!Logging.On)
                //    return;
                //Logging.Exit(Logging.Sockets, (object) this, "Start", (string) null);
            }
            else
            {
                this.m_ServerSocket.Bind((EndPoint) this.m_ServerSocketEP);
                try
                {
                    this.m_ServerSocket.Listen(backlog);
                }
                catch (SocketException ex)
                {
                    this.Stop();
                    throw;
                }
                this.m_Active = true;
                //if (!Logging.On)
                //    return;
                //Logging.Exit(Logging.Sockets, (object) this, "Start", (string) null);
            }
        }

        /// <summary>
        /// Closes the listener.
        /// </summary>
        /// <exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode"/> property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void Stop()
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "Stop", (string) null);
            if (this.m_ServerSocket != null)
            {
                this.m_ServerSocket.Close();
                this.m_ServerSocket = (Socket) null;
            }
            this.m_Active = false;
            this.m_ServerSocket = new Socket(this.m_ServerSocketEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (this.m_ExclusiveAddressUse)
                this.m_ServerSocket.ExclusiveAddressUse = true;
            //if (!Logging.On)
            //    return;
            //Logging.Exit(Logging.Sockets, (object) this, "Stop", (string) null);
        }

        /// <summary>
        /// Determines if there are pending connection requests.
        /// </summary>
        /// 
        /// <returns>
        /// true if connections are pending; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.UdtListener.Start"/>. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public bool Pending()
        {
            if (!this.m_Active)
                throw new InvalidOperationException("net_stopped");
            else
                return this.m_ServerSocket.Poll(0, SelectMode.SelectRead);
        }

        /// <summary>
        /// Accepts a pending connection request.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Net.Sockets.Socket"/> used to send and receive data.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.UdtListener.Start"/>. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public Socket AcceptSocket()
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "AcceptSocket", (string) null);
            if (!this.m_Active)
                throw new InvalidOperationException("net_stopped");
            Socket socket = this.m_ServerSocket.Accept();
            //if (Logging.On)
            //    Logging.Exit(Logging.Sockets, (object) this, "AcceptSocket", (object) socket);
            return socket;
        }

        /// <summary>
        /// Accepts a pending connection request.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Net.Sockets.UdtClient"/> used to send and receive data.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.UdtListener.Start"/>. </exception><exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode"/> property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public UdtClient AcceptUdtClient()
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "AcceptUdtClient", (string) null);
            if (!this.m_Active)
                throw new InvalidOperationException("net_stopped");
            UdtClient UdtClient = new UdtClient(this.m_ServerSocket.Accept());
            //if (Logging.On)
            //    Logging.Exit(Logging.Sockets, (object) this, "AcceptUdtClient", (object) UdtClient);
            return UdtClient;
        }

        /// <summary>
        /// Begins an asynchronous operation to accept an incoming connection attempt.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> that references the asynchronous creation of the <see cref="T:System.Net.Sockets.Socket"/>.
        /// </returns>
        /// <param name="callback">An <see cref="T:System.AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param><param name="state">A user-defined object containing information about the accept operation. This object is passed to the <paramref name="callback"/> delegate when the operation is complete.</param><exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state)
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "BeginAcceptSocket", (string) null);
            if (!this.m_Active)
                throw new InvalidOperationException("net_stopped");
            IAsyncResult asyncResult = this.m_ServerSocket.BeginAccept(callback, state);
            //if (Logging.On)
            //    Logging.Exit(Logging.Sockets, (object) this, "BeginAcceptSocket", (string) null);
            return asyncResult;
        }

        /// <summary>
        /// Asynchronously accepts an incoming connection attempt and creates a new <see cref="T:System.Net.Sockets.Socket"/> to handle remote host communication.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Net.Sockets.Socket"/>.The <see cref="T:System.Net.Sockets.Socket"/> used to send and receive data.
        /// </returns>
        /// <param name="asyncResult">An <see cref="T:System.IAsyncResult"/> returned by a call to the <see cref="M:System.Net.Sockets.UdtListener.BeginAcceptSocket(System.AsyncCallback,System.Object)"/>  method.</param><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><exception cref="T:System.ArgumentNullException">The <paramref name="asyncResult"/> parameter is null. </exception><exception cref="T:System.ArgumentException">The <paramref name="asyncResult"/> parameter was not created by a call to the <see cref="M:System.Net.Sockets.UdtListener.BeginAcceptSocket(System.AsyncCallback,System.Object)"/> method. </exception><exception cref="T:System.InvalidOperationException">The <see cref="M:System.Net.Sockets.UdtListener.EndAcceptSocket(System.IAsyncResult)"/> method was previously called. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the <see cref="T:System.Net.Sockets.Socket"/>. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public Socket EndAcceptSocket(IAsyncResult asyncResult)
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "EndAcceptSocket", (string) null);
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");
            LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
            Socket socket1 = lazyAsyncResult == null ? (Socket) null : lazyAsyncResult.AsyncObject as Socket;
            if (socket1 == null)
                throw new ArgumentException("net_io_invalidasyncresult", "asyncResult");
            Socket socket2 = socket1.EndAccept(asyncResult);
            //if (Logging.On)
            //    Logging.Exit(Logging.Sockets, (object) this, "EndAcceptSocket", (object) socket2);
            return socket2;
        }

        /// <summary>
        /// Begins an asynchronous operation to accept an incoming connection attempt.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> that references the asynchronous creation of the <see cref="T:System.Net.Sockets.UdtClient"/>.
        /// </returns>
        /// <param name="callback">An <see cref="T:System.AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param><param name="state">A user-defined object containing information about the accept operation. This object is passed to the <paramref name="callback"/> delegate when the operation is complete.</param><exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginAcceptUdtClient(AsyncCallback callback, object state)
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "BeginAcceptUdtClient", (string) null);
            if (!this.m_Active)
                throw new InvalidOperationException("net_stopped");
            IAsyncResult asyncResult = this.m_ServerSocket.BeginAccept(callback, state);
            //if (Logging.On)
            //    Logging.Exit(Logging.Sockets, (object) this, "BeginAcceptUdtClient", (string) null);
            return asyncResult;
        }

        /// <summary>
        /// Asynchronously accepts an incoming connection attempt and creates a new <see cref="T:System.Net.Sockets.UdtClient"/> to handle remote host communication.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Net.Sockets.UdtClient"/>.The <see cref="T:System.Net.Sockets.UdtClient"/> used to send and receive data.
        /// </returns>
        /// <param name="asyncResult">An <see cref="T:System.IAsyncResult"/> returned by a call to the <see cref="M:System.Net.Sockets.UdtListener.BeginAcceptUdtClient(System.AsyncCallback,System.Object)"/> method.</param><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public UdtClient EndAcceptUdtClient(IAsyncResult asyncResult)
        {
            //if (Logging.On)
            //    Logging.Enter(Logging.Sockets, (object) this, "EndAcceptUdtClient", (string) null);
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");
            LazyAsyncResult lazyAsyncResult = asyncResult as LazyAsyncResult;
            Socket socket = lazyAsyncResult == null ? (Socket) null : lazyAsyncResult.AsyncObject as Socket;
            if (socket == null)
                throw new ArgumentException("net_io_invalidasyncresult", "asyncResult");
            Socket acceptedSocket = socket.EndAccept(asyncResult);
            //if (Logging.On)
            //    Logging.Exit(Logging.Sockets, (object) this, "EndAcceptUdtClient", (object) acceptedSocket);
            return new UdtClient(acceptedSocket);
        }

        /// <summary>
        /// Accepts a pending connection request as an asynchronous operation.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result"/> property on the task object returns a <see cref="T:System.Net.Sockets.Socket"/> used to send and receive data.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.UdtListener.Start"/>. </exception>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<Socket> AcceptSocketAsync()
        {
            return Task<Socket>.Factory.FromAsync(
                new Func<AsyncCallback, object, IAsyncResult>(this.BeginAcceptSocket),
                new Func<IAsyncResult, Socket>(this.EndAcceptSocket), (object) null);
        }

        /// <summary>
        /// Accepts a pending connection request as an asynchronous operation.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>The task object representing the asynchronous operation. The <see cref="P:System.Threading.Tasks.Task`1.Result"/> property on the task object returns a <see cref="T:System.Net.Sockets.UdtClient"/> used to send and receive data.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The listener has not been started with a call to <see cref="M:System.Net.Sockets.UdtListener.Start"/>. </exception><exception cref="T:System.Net.Sockets.SocketException">Use the <see cref="P:System.Net.Sockets.SocketException.ErrorCode"/> property to obtain the specific error code. When you have obtained this code, you can refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error. </exception>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<UdtClient> AcceptUdtClientAsync()
        {
            return
                Task<UdtClient>.Factory.FromAsync(
                    new Func<AsyncCallback, object, IAsyncResult>(this.BeginAcceptUdtClient),
                    new Func<IAsyncResult, UdtClient>(this.EndAcceptUdtClient), (object) null);
        }
    }
}

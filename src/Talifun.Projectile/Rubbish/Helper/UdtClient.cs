using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace Talifun.Projectile.Rubbish.Helper
{
    public class UdtClient : IDisposable
    {
        private static IPEndPoint Any = new IPEndPoint(IPAddress.Any, 0);
        private static IPEndPoint IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);

        private byte[] m_Buffer = new byte[65536];
        private AddressFamily m_Family = AddressFamily.InterNetwork;
        private Socket m_ClientSocket;
        private bool m_Active;
        private bool m_CleanedUp;
        private bool m_IsBroadcast;
        private const int MaxUDPSize = 65536;

        private static bool ValidateTcpPort(int port)
        {
            if (port >= 0)
                return port <= (int)ushort.MaxValue;
            else
                return false;
        }

        public static bool ValidateRange(int actual, int fromAllowed, int toAllowed)
        {
            if (actual >= fromAllowed)
                return actual <= toAllowed;
            else
                return false;
        }

        /// <summary>
        /// Gets or sets the underlying network <see cref="T:System.Net.Sockets.Socket"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The underlying Network <see cref="T:System.Net.Sockets.Socket"/>.
        /// </returns>
        public Socket Client
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] get { return this.m_ClientSocket; }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] set { this.m_ClientSocket = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a default remote host has been established.
        /// </summary>
        /// 
        /// <returns>
        /// true if a connection is active; otherwise, false.
        /// </returns>
        protected bool Active
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] get { return this.m_Active; }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] set { this.m_Active = value; }
        }

        /// <summary>
        /// Gets the amount of data received from the network that is available to read.
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes of data received from the network.
        /// </returns>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error occurred while attempting to access the socket. See the Remarks section for more information. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public int Available
        {
            get { return this.m_ClientSocket.Available; }
        }

        /// <summary>
        /// Gets or sets a value that specifies the Time to Live (TTL) value of Internet Protocol (IP) packets sent by the <see cref="T:System.Net.Sockets.UdtClient"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The TTL value.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public short Ttl
        {
            get { return this.m_ClientSocket.Ttl; }
            set { this.m_ClientSocket.Ttl = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="T:System.Boolean"/> value that specifies whether the <see cref="T:System.Net.Sockets.UdtClient"/> allows Internet Protocol (IP) datagrams to be fragmented.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:System.Net.Sockets.UdtClient"/> allows datagram fragmentation; otherwise, false. The default is true.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">This property can be set only for sockets that use the <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork"/> flag or the <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6"/> flag. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public bool DontFragment
        {
            get { return this.m_ClientSocket.DontFragment; }
            set { this.m_ClientSocket.DontFragment = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="T:System.Boolean"/> value that specifies whether outgoing multicast packets are delivered to the sending application.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:System.Net.Sockets.UdtClient"/> receives outgoing multicast packets; otherwise, false.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public bool MulticastLoopback
        {
            get { return this.m_ClientSocket.MulticastLoopback; }
            set { this.m_ClientSocket.MulticastLoopback = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="T:System.Boolean"/> value that specifies whether the <see cref="T:System.Net.Sockets.UdtClient"/> may send or receive broadcast packets.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:System.Net.Sockets.UdtClient"/> allows broadcast packets; otherwise, false. The default is false.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public bool EnableBroadcast
        {
            get { return this.m_ClientSocket.EnableBroadcast; }
            set { this.m_ClientSocket.EnableBroadcast = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="T:System.Boolean"/> value that specifies whether the <see cref="T:System.Net.Sockets.UdtClient"/> allows only one client to use a port.
        /// </summary>
        /// 
        /// <returns>
        /// true if the <see cref="T:System.Net.Sockets.UdtClient"/> allows only one client to use a specific port; otherwise, false. The default is true for Windows Server 2003 and Windows XP Service Pack 2 and later, and false for all other versions.
        /// </returns>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the underlying socket.</exception><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public bool ExclusiveAddressUse
        {
            get { return this.m_ClientSocket.ExclusiveAddressUse; }
            set { this.m_ClientSocket.ExclusiveAddressUse = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.UdtClient"/> class.
        /// </summary>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UdtClient()
            : this(AddressFamily.InterNetwork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.UdtClient"/> class.
        /// </summary>
        /// <param name="family">One of the <see cref="T:System.Net.Sockets.AddressFamily"/> values that specifies the addressing scheme of the socket. </param><exception cref="T:System.ArgumentException"><paramref name="family"/> is not <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork"/> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6"/>. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        public UdtClient(AddressFamily family)
        {
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException(string.Format("'{0}' Client can only accept InterNetwork or InterNetworkV6 addresses.", new object[1]
                {
                    (object) "UDP"
                }), "family");
            }
            else
            {
                this.m_Family = family;
                this.createClientSocket();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.UdtClient"/> class and binds it to the local port number provided.
        /// </summary>
        /// <param name="port">The local port number from which you intend to communicate. </param><exception cref="T:System.ArgumentOutOfRangeException">The <paramref name="port"/> parameter is greater than <see cref="F:System.Net.IPEndPoint.MaxPort"/> or less than <see cref="F:System.Net.IPEndPoint.MinPort"/>. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UdtClient(int port)
            : this(port, AddressFamily.InterNetwork)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.UdtClient"/> class and binds it to the local port number provided.
        /// </summary>
        /// <param name="port">The port on which to listen for incoming connection attempts. </param><param name="family">One of the <see cref="T:System.Net.Sockets.AddressFamily"/> values that specifies the addressing scheme of the socket. </param><exception cref="T:System.ArgumentException"><paramref name="family"/> is not <see cref="F:System.Net.Sockets.AddressFamily.InterNetwork"/> or <see cref="F:System.Net.Sockets.AddressFamily.InterNetworkV6"/>. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is greater than <see cref="F:System.Net.IPEndPoint.MaxPort"/> or less than <see cref="F:System.Net.IPEndPoint.MinPort"/>. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        public UdtClient(int port, AddressFamily family)
        {
            if (!ValidateTcpPort(port))
                throw new ArgumentOutOfRangeException("port");
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
                throw new ArgumentException("net_protocol_invalid_family", "family");
            this.m_Family = family;
            IPEndPoint ipEndPoint = this.m_Family != AddressFamily.InterNetwork
                ? new IPEndPoint(IPAddress.IPv6Any, port)
                : new IPEndPoint(IPAddress.Any, port);
            this.createClientSocket();
            this.Client.Bind((EndPoint) ipEndPoint);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.UdtClient"/> class and binds it to the specified local endpoint.
        /// </summary>
        /// <param name="localEP">An <see cref="T:System.Net.IPEndPoint"/> that respresents the local endpoint to which you bind the UDP connection. </param><exception cref="T:System.ArgumentNullException"><paramref name="localEP"/> is null. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        public UdtClient(IPEndPoint localEP)
        {
            if (localEP == null)
                throw new ArgumentNullException("localEP");
            this.m_Family = localEP.AddressFamily;
            this.createClientSocket();
            this.Client.Bind((EndPoint) localEP);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.UdtClient"/> class and establishes a default remote host.
        /// </summary>
        /// <param name="hostname">The name of the remote DNS host to which you intend to connect. </param><param name="port">The remote port number to which you intend to connect. </param><exception cref="T:System.ArgumentNullException"><paramref name="hostname"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is not between <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="F:System.Net.IPEndPoint.MaxPort"/>. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        public UdtClient(string hostname, int port)
        {
            if (hostname == null)
                throw new ArgumentNullException("hostname");
            if (!ValidateTcpPort(port))
                throw new ArgumentOutOfRangeException("port");
            this.Connect(hostname, port);
        }

        public UdtClient(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");
            this.m_Family = socket.AddressFamily;
            this.Client = socket;
        }

        /// <summary>
        /// Enables or disables Network Address Translation (NAT) traversal on a <see cref="T:System.Net.Sockets.UdtClient"/> instance.
        /// </summary>
        /// <param name="allowed">A Boolean value that specifies whether to enable or disable NAT traversal.</param>
        public void AllowNatTraversal(bool allowed)
        {
            if (allowed)
                this.m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            else
                this.m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
        }

        /// <summary>
        /// Closes the UDP connection.
        /// </summary>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Close()
        {
            this.Dispose(true);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Net.Sockets.UdtClient"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            this.FreeResources();
            GC.SuppressFinalize((object) this);
        }

        internal static bool IsFatal(Exception exception)
        {
            if (exception == null)
                return false;
            if (!(exception is OutOfMemoryException) && !(exception is StackOverflowException))
                return exception is ThreadAbortException;
            else
                return true;
        }

        /// <summary>
        /// Establishes a default remote host using the specified host name and port number.
        /// </summary>
        /// <param name="hostname">The DNS name of the remote host to which you intend send data. </param><param name="port">The port number on the remote host to which you intend to send data. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is not between <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="F:System.Net.IPEndPoint.MaxPort"/>. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void Connect(string hostname, int port)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (hostname == null)
                throw new ArgumentNullException("hostname");
            if (!ValidateTcpPort(port))
                throw new ArgumentOutOfRangeException("port");
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
            Exception exception = (Exception) null;
            Socket socket1 = (Socket) null;
            Socket socket2 = (Socket) null;
            try
            {
                if (this.m_ClientSocket == null)
                {
                    if (Socket.OSSupportsIPv4)
                        socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    if (Socket.OSSupportsIPv6)
                        socket1 = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                }
                foreach (IPAddress address in hostAddresses)
                {
                    try
                    {
                        if (this.m_ClientSocket == null)
                        {
                            if (address.AddressFamily == AddressFamily.InterNetwork && socket2 != null)
                            {
                                socket2.Connect(address, port);
                                this.m_ClientSocket = socket2;
                                if (socket1 != null)
                                    socket1.Close();
                            }
                            else if (socket1 != null)
                            {
                                socket1.Connect(address, port);
                                this.m_ClientSocket = socket1;
                                if (socket2 != null)
                                    socket2.Close();
                            }
                            this.m_Family = address.AddressFamily;
                            this.m_Active = true;
                            break;
                        }
                        else if (address.AddressFamily == this.m_Family)
                        {
                            this.Connect(new IPEndPoint(address, port));
                            this.m_Active = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (IsFatal(ex))
                            throw;
                        else
                            exception = ex;
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsFatal(ex))
                    throw;
                else
                    exception = ex;
            }
            finally
            {
                if (!this.m_Active)
                {
                    if (socket1 != null)
                        socket1.Close();
                    if (socket2 != null)
                        socket2.Close();
                    if (exception != null)
                        throw exception;
                    else
                        throw new SocketException((int)SocketError.NotConnected);
                }
            }
        }

        /// <summary>
        /// Establishes a default remote host using the specified IP address and port number.
        /// </summary>
        /// <param name="addr">The <see cref="T:System.Net.IPAddress"/> of the remote host to which you intend to send data. </param><param name="port">The port number to which you intend send data. </param><exception cref="T:System.ObjectDisposedException"><see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><exception cref="T:System.ArgumentNullException"><paramref name="addr"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is not between <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="F:System.Net.IPEndPoint.MaxPort"/>. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void Connect(IPAddress addr, int port)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (addr == null)
                throw new ArgumentNullException("addr");
            if (!ValidateTcpPort(port))
                throw new ArgumentOutOfRangeException("port");
            this.Connect(new IPEndPoint(addr, port));
        }

        /// <summary>
        /// Establishes a default remote host using the specified network endpoint.
        /// </summary>
        /// <param name="endPoint">An <see cref="T:System.Net.IPEndPoint"/> that specifies the network endpoint to which you intend to send data. </param><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><exception cref="T:System.ArgumentNullException"><paramref name="endPoint"/> is null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void Connect(IPEndPoint endPoint)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (endPoint == null)
                throw new ArgumentNullException("endPoint");
            this.CheckForBroadcast(endPoint.Address);
            this.Client.Connect((EndPoint) endPoint);
            this.m_Active = true;
        }

        /// <summary>
        /// Sends a UDP datagram to the host at the specified remote endpoint.
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes sent.
        /// </returns>
        /// <param name="dgram">An array of type <see cref="T:System.Byte"/> that specifies the UDP datagram that you intend to send, represented as an array of bytes. </param><param name="bytes">The number of bytes in the datagram. </param><param name="endPoint">An <see cref="T:System.Net.IPEndPoint"/> that represents the host and port to which to send the datagram. </param><exception cref="T:System.ArgumentNullException"><paramref name="dgram"/> is null. </exception><exception cref="T:System.InvalidOperationException"><see cref="T:System.Net.Sockets.UdtClient"/> has already established a default remote host. </exception><exception cref="T:System.ObjectDisposedException"><see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public int Send(byte[] dgram, int bytes, IPEndPoint endPoint)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (dgram == null)
                throw new ArgumentNullException("dgram");
            if (this.m_Active && endPoint != null)
                throw new InvalidOperationException("net_udpconnected");
            if (endPoint == null)
                return this.Client.Send(dgram, 0, bytes, SocketFlags.None);
            this.CheckForBroadcast(endPoint.Address);
            return this.Client.SendTo(dgram, 0, bytes, SocketFlags.None, (EndPoint) endPoint);
        }

        /// <summary>
        /// Sends a UDP datagram to a specified port on a specified remote host.
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes sent.
        /// </returns>
        /// <param name="dgram">An array of type <see cref="T:System.Byte"/> that specifies the UDP datagram that you intend to send represented as an array of bytes. </param><param name="bytes">The number of bytes in the datagram. </param><param name="hostname">The name of the remote host to which you intend to send the datagram. </param><param name="port">The remote port number with which you intend to communicate. </param><exception cref="T:System.ArgumentNullException"><paramref name="dgram"/> is null. </exception><exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdtClient"/> has already established a default remote host. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public int Send(byte[] dgram, int bytes, string hostname, int port)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (dgram == null)
                throw new ArgumentNullException("dgram");
            if (this.m_Active && (hostname != null || port != 0))
                throw new InvalidOperationException("net_udpconnected");
            if (hostname == null || port == 0)
                return this.Client.Send(dgram, 0, bytes, SocketFlags.None);
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
            int index = 0;
            while (index < hostAddresses.Length && hostAddresses[index].AddressFamily != this.m_Family)
                ++index;
            if (hostAddresses.Length == 0 || index == hostAddresses.Length)
                throw new ArgumentException("net_invalidAddressList", "hostname");
            this.CheckForBroadcast(hostAddresses[index]);
            IPEndPoint ipEndPoint = new IPEndPoint(hostAddresses[index], port);
            return this.Client.SendTo(dgram, 0, bytes, SocketFlags.None, (EndPoint) ipEndPoint);
        }

        /// <summary>
        /// Sends a UDP datagram to a remote host.
        /// </summary>
        /// 
        /// <returns>
        /// The number of bytes sent.
        /// </returns>
        /// <param name="dgram">An array of type <see cref="T:System.Byte"/> that specifies the UDP datagram that you intend to send represented as an array of bytes. </param><param name="bytes">The number of bytes in the datagram. </param><exception cref="T:System.ArgumentNullException"><paramref name="dgram"/> is null. </exception><exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdtClient"/> has already established a default remote host. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public int Send(byte[] dgram, int bytes)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (dgram == null)
                throw new ArgumentNullException("dgram");
            if (!this.m_Active)
                throw new InvalidOperationException("net_notconnected");
            else
                return this.Client.Send(dgram, 0, bytes, SocketFlags.None);
        }

        /// <summary>
        /// Sends a datagram to a destination asynchronously. The destination is specified by a <see cref="T:System.Net.EndPoint"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> object that references the asynchronous send.
        /// </returns>
        /// <param name="datagram">A <see cref="T:System.Byte"/> array that contains the data to be sent.</param><param name="bytes">The number of bytes to send.</param><param name="endPoint">The <see cref="T:System.Net.EndPoint"/> that represents the destination for the data.</param><param name="requestCallback">An <see cref="T:System.AsyncCallback"/> delegate that references the method to invoke when the operation is complete. </param><param name="state">A user-defined object that contains information about the send operation. This object is passed to the <paramref name="requestCallback"/> delegate when the operation is complete.</param>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback,
            object state)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (datagram == null)
                throw new ArgumentNullException("datagram");
            if (bytes > datagram.Length)
                throw new ArgumentOutOfRangeException("bytes");
            if (this.m_Active && endPoint != null)
                throw new InvalidOperationException("net_udpconnected");
            if (endPoint == null)
                return this.Client.BeginSend(datagram, 0, bytes, SocketFlags.None, requestCallback, state);
            this.CheckForBroadcast(endPoint.Address);
            return this.Client.BeginSendTo(datagram, 0, bytes, SocketFlags.None, (EndPoint) endPoint, requestCallback,
                state);
        }

        /// <summary>
        /// Sends a datagram to a destination asynchronously. The destination is specified by the host name and port number.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> object that references the asynchronous send.
        /// </returns>
        /// <param name="datagram">A <see cref="T:System.Byte"/> array that contains the data to be sent.</param><param name="bytes">The number of bytes to send.</param><param name="hostname">The destination host.</param><param name="port">The destination port number.</param><param name="requestCallback">An <see cref="T:System.AsyncCallback"/> delegate that references the method to invoke when the operation is complete. </param><param name="state">A user-defined object that contains information about the send operation. This object is passed to the <paramref name="requestCallback"/> delegate when the operation is complete.</param>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, string hostname, int port,
            AsyncCallback requestCallback, object state)
        {
            if (this.m_Active && (hostname != null || port != 0))
                throw new InvalidOperationException("net_udpconnected");
            IPEndPoint endPoint = (IPEndPoint) null;
            if (hostname != null && port != 0)
            {
                IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
                int index = 0;
                while (index < hostAddresses.Length && hostAddresses[index].AddressFamily != this.m_Family)
                    ++index;
                if (hostAddresses.Length == 0 || index == hostAddresses.Length)
                    throw new ArgumentException("net_invalidAddressList", "hostname");
                this.CheckForBroadcast(hostAddresses[index]);
                endPoint = new IPEndPoint(hostAddresses[index], port);
            }
            return this.BeginSend(datagram, bytes, endPoint, requestCallback, state);
        }

        /// <summary>
        /// Sends a datagram to a remote host asynchronously. The destination was specified previously by a call to <see cref="Overload:System.Net.Sockets.UdtClient.Connect"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> object that references the asynchronous send.
        /// </returns>
        /// <param name="datagram">A <see cref="T:System.Byte"/> array that contains the data to be sent.</param><param name="bytes">The number of bytes to send.</param><param name="requestCallback">An <see cref="T:System.AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param><param name="state">A user-defined object that contains information about the send operation. This object is passed to the <paramref name="requestCallback"/> delegate when the operation is complete.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, AsyncCallback requestCallback, object state)
        {
            return this.BeginSend(datagram, bytes, (IPEndPoint) null, requestCallback, state);
        }

        /// <summary>
        /// Ends a pending asynchronous send.
        /// </summary>
        /// 
        /// <returns>
        /// If successful, the number of bytes sent to the <see cref="T:System.Net.Sockets.UdtClient"/>.
        /// </returns>
        /// <param name="asyncResult">An <see cref="T:System.IAsyncResult"/> object returned by a call to <see cref="Overload:System.Net.Sockets.UdtClient.BeginSend"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="asyncResult"/> is null. </exception><exception cref="T:System.ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="M:System.Net.Sockets.Socket.BeginSend(System.Byte[],System.Int32,System.Int32,System.Net.Sockets.SocketFlags,System.AsyncCallback,System.Object)"/> method. </exception><exception cref="T:System.InvalidOperationException"><see cref="M:System.Net.Sockets.Socket.EndSend(System.IAsyncResult)"/> was previously called for the asynchronous read. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the underlying socket. See the Remarks section for more information. </exception><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception>
        public int EndSend(IAsyncResult asyncResult)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (this.m_Active)
                return this.Client.EndSend(asyncResult);
            else
                return this.Client.EndSendTo(asyncResult);
        }

        /// <summary>
        /// Returns a UDP datagram that was sent by a remote host.
        /// </summary>
        /// 
        /// <returns>
        /// An array of type <see cref="T:System.Byte"/> that contains datagram data.
        /// </returns>
        /// <param name="remoteEP">An <see cref="T:System.Net.IPEndPoint"/> that represents the remote host from which the data was sent. </param><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/>  has been closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Net.SocketPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public byte[] Receive(ref IPEndPoint remoteEP)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            EndPoint remoteEP1 = this.m_Family != AddressFamily.InterNetwork
                ? (EndPoint) IPv6Any
                : (EndPoint) Any;
            int count = this.Client.ReceiveFrom(this.m_Buffer, 65536, SocketFlags.None, ref remoteEP1);
            remoteEP = (IPEndPoint) remoteEP1;
            if (count >= 65536)
                return this.m_Buffer;
            byte[] numArray = new byte[count];
            Buffer.BlockCopy((Array) this.m_Buffer, 0, (Array) numArray, 0, count);
            return numArray;
        }

        /// <summary>
        /// Receives a datagram from a remote host asynchronously.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.IAsyncResult"/> object that references the asynchronous receive.
        /// </returns>
        /// <param name="requestCallback">An <see cref="T:System.AsyncCallback"/> delegate that references the method to invoke when the operation is complete. </param><param name="state">A user-defined object that contains information about the receive operation. This object is passed to the <paramref name="requestCallback"/> delegate when the operation is complete.</param>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginReceive(AsyncCallback requestCallback, object state)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            EndPoint remoteEP = this.m_Family != AddressFamily.InterNetwork
                ? (EndPoint) IPv6Any
                : (EndPoint) Any;
            return this.Client.BeginReceiveFrom(this.m_Buffer, 0, 65536, SocketFlags.None, ref remoteEP, requestCallback,
                state);
        }

        /// <summary>
        /// Ends a pending asynchronous receive.
        /// </summary>
        /// 
        /// <returns>
        /// If successful, the number of bytes received. If unsuccessful, this method returns 0.
        /// </returns>
        /// <param name="asyncResult">An <see cref="T:System.IAsyncResult"/> object returned by a call to <see cref="M:System.Net.Sockets.UdtClient.BeginReceive(System.AsyncCallback,System.Object)"/>.</param><param name="remoteEP">The specified remote endpoint.</param><exception cref="T:System.ArgumentNullException"><paramref name="asyncResult"/> is null. </exception><exception cref="T:System.ArgumentException"><paramref name="asyncResult"/> was not returned by a call to the <see cref="M:System.Net.Sockets.UdtClient.BeginReceive(System.AsyncCallback,System.Object)"/> method. </exception><exception cref="T:System.InvalidOperationException"><see cref="M:System.Net.Sockets.UdtClient.EndReceive(System.IAsyncResult,System.Net.IPEndPoint@)"/> was previously called for the asynchronous read. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the underlying <see cref="T:System.Net.Sockets.Socket"/>. See the Remarks section for more information. </exception><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception>
        public byte[] EndReceive(IAsyncResult asyncResult, ref IPEndPoint remoteEP)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            EndPoint endPoint = this.m_Family != AddressFamily.InterNetwork
                ? (EndPoint) IPv6Any
                : (EndPoint) Any;
            int count = this.Client.EndReceiveFrom(asyncResult, ref endPoint);
            remoteEP = (IPEndPoint) endPoint;
            if (count >= 65536)
                return this.m_Buffer;
            byte[] numArray = new byte[count];
            Buffer.BlockCopy((Array) this.m_Buffer, 0, (Array) numArray, 0, count);
            return numArray;
        }

        /// <summary>
        /// Adds a <see cref="T:System.Net.Sockets.UdtClient"/> to a multicast group.
        /// </summary>
        /// <param name="multicastAddr">The multicast <see cref="T:System.Net.IPAddress"/> of the group you want to join. </param><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><exception cref="T:System.ArgumentException">The IP address is not compatible with the <see cref="T:System.Net.Sockets.AddressFamily"/> value that defines the addressing scheme of the socket. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void JoinMulticastGroup(IPAddress multicastAddr)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (multicastAddr == null)
                throw new ArgumentNullException("multicastAddr");
            if (multicastAddr.AddressFamily != this.m_Family)
                throw new ArgumentException(string.Format("Multicast family is not the same as the family of the '{0}' Client.", new object[1]
                {
                    (object) "UDP"
                }), "multicastAddr");
            else if (this.m_Family == AddressFamily.InterNetwork)
                this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                    (object) new MulticastOption(multicastAddr));
            else
                this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
                    (object) new IPv6MulticastOption(multicastAddr));
        }

        /// <summary>
        /// Adds a <see cref="T:System.Net.Sockets.UdtClient"/> to a multicast group.
        /// </summary>
        /// <param name="multicastAddr">The multicast <see cref="T:System.Net.IPAddress"/> of the group you want to join.</param><param name="localAddress">The local <see cref="T:System.Net.IPAddress"/>.</param><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void JoinMulticastGroup(IPAddress multicastAddr, IPAddress localAddress)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (this.m_Family != AddressFamily.InterNetwork)
                throw new SocketException((int)SocketError.OperationNotSupported);
            this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                (object) new MulticastOption(multicastAddr, localAddress));
        }

        /// <summary>
        /// Adds a <see cref="T:System.Net.Sockets.UdtClient"/> to a multicast group.
        /// </summary>
        /// <param name="ifindex">The interface index associated with the local IP address on which to join the multicast group.</param><param name="multicastAddr">The multicast <see cref="T:System.Net.IPAddress"/> of the group you want to join. </param><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void JoinMulticastGroup(int ifindex, IPAddress multicastAddr)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (multicastAddr == null)
                throw new ArgumentNullException("multicastAddr");
            if (ifindex < 0)
                throw new ArgumentException("net_value_cannot_be_negative", "ifindex");
            if (this.m_Family != AddressFamily.InterNetworkV6)
                throw new SocketException((int)SocketError.OperationNotSupported);
            this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
                (object) new IPv6MulticastOption(multicastAddr, (long) ifindex));
        }

        /// <summary>
        /// Adds a <see cref="T:System.Net.Sockets.UdtClient"/> to a multicast group with the specified Time to Live (TTL).
        /// </summary>
        /// <param name="multicastAddr">The <see cref="T:System.Net.IPAddress"/> of the multicast group to join. </param><param name="timeToLive">The Time to Live (TTL), measured in router hops. </param><exception cref="T:System.ArgumentOutOfRangeException">The TTL provided is not between 0 and 255 </exception><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><exception cref="T:System.ArgumentNullException"><paramref name="multicastAddr"/> is null.</exception><exception cref="T:System.ArgumentException">The IP address is not compatible with the <see cref="T:System.Net.Sockets.AddressFamily"/> value that defines the addressing scheme of the socket. </exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void JoinMulticastGroup(IPAddress multicastAddr, int timeToLive)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (multicastAddr == null)
                throw new ArgumentNullException("multicastAddr");
            if (!ValidateRange(timeToLive, 0, (int) byte.MaxValue))
                throw new ArgumentOutOfRangeException("timeToLive");
            this.JoinMulticastGroup(multicastAddr);
            this.Client.SetSocketOption(
                this.m_Family == AddressFamily.InterNetwork ? SocketOptionLevel.IP : SocketOptionLevel.IPv6,
                SocketOptionName.MulticastTimeToLive, timeToLive);
        }

        /// <summary>
        /// Leaves a multicast group.
        /// </summary>
        /// <param name="multicastAddr">The <see cref="T:System.Net.IPAddress"/> of the multicast group to leave. </param><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><exception cref="T:System.ArgumentException">The IP address is not compatible with the <see cref="T:System.Net.Sockets.AddressFamily"/> value that defines the addressing scheme of the socket. </exception><exception cref="T:System.ArgumentNullException"><paramref name="multicastAddr"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void DropMulticastGroup(IPAddress multicastAddr)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (multicastAddr == null)
                throw new ArgumentNullException("multicastAddr");
            if (multicastAddr.AddressFamily != this.m_Family)
                throw new ArgumentException(string.Format("Multicast family is not the same as the family of the '{0}' Client.", new object[1]
                {
                    (object) "UDP"
                }), "multicastAddr");
            else if (this.m_Family == AddressFamily.InterNetwork)
                this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership,
                    (object) new MulticastOption(multicastAddr));
            else
                this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership,
                    (object) new IPv6MulticastOption(multicastAddr));
        }

        /// <summary>
        /// Leaves a multicast group.
        /// </summary>
        /// <param name="multicastAddr">The <see cref="T:System.Net.IPAddress"/> of the multicast group to leave. </param><param name="ifindex">The local address of the multicast group to leave.</param><exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/> has been closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception><exception cref="T:System.ArgumentException">The IP address is not compatible with the <see cref="T:System.Net.Sockets.AddressFamily"/> value that defines the addressing scheme of the socket. </exception><exception cref="T:System.ArgumentNullException"><paramref name="multicastAddr"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public void DropMulticastGroup(IPAddress multicastAddr, int ifindex)
        {
            if (this.m_CleanedUp)
                throw new ObjectDisposedException(this.GetType().FullName);
            if (multicastAddr == null)
                throw new ArgumentNullException("multicastAddr");
            if (ifindex < 0)
                throw new ArgumentException("net_value_cannot_be_negative", "ifindex");
            if (this.m_Family != AddressFamily.InterNetworkV6)
                throw new SocketException((int)SocketError.OperationNotSupported);
            this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership,
                (object) new IPv6MulticastOption(multicastAddr, (long) ifindex));
        }

        /// <summary>
        /// Sends a UDP datagram asynchronously to a remote host.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>.
        /// </returns>
        /// <param name="datagram">An array of type <see cref="T:System.Byte"/> that specifies the UDP datagram that you intend to send represented as an array of bytes.</param><param name="bytes">The number of bytes in the datagram.</param><exception cref="T:System.ArgumentNullException"><paramref name="dgram"/> is null. </exception><exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdtClient"/> has already established a default remote host. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<int> SendAsync(byte[] datagram, int bytes)
        {
            return
                Task<int>.Factory.FromAsync<byte[], int>(
                    new Func<byte[], int, AsyncCallback, object, IAsyncResult>(this.BeginSend),
                    new Func<IAsyncResult, int>(this.EndSend), datagram, bytes, (object) null);
        }

        /// <summary>
        /// Sends a UDP datagram asynchronously to a remote host.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>.
        /// </returns>
        /// <param name="datagram">An array of type <see cref="T:System.Byte"/> that specifies the UDP datagram that you intend to send represented as an array of bytes.</param><param name="bytes">The number of bytes in the datagram.</param><param name="endPoint">An <see cref="T:System.Net.IPEndPoint"/> that represents the host and port to which to send the datagram.</param><exception cref="T:System.ArgumentNullException"><paramref name="dgram"/> is null. </exception><exception cref="T:System.InvalidOperationException"><see cref="T:System.Net.Sockets.UdtClient"/> has already established a default remote host. </exception><exception cref="T:System.ObjectDisposedException"><see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            return
                Task<int>.Factory.FromAsync<byte[], int, IPEndPoint>(
                    new Func<byte[], int, IPEndPoint, AsyncCallback, object, IAsyncResult>(this.BeginSend),
                    new Func<IAsyncResult, int>(this.EndSend), datagram, bytes, endPoint, (object) null);
        }

        /// <summary>
        /// Sends a UDP datagram asynchronously to a remote host.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>.
        /// </returns>
        /// <param name="datagram">An array of type <see cref="T:System.Byte"/> that specifies the UDP datagram that you intend to send represented as an array of bytes.</param><param name="bytes">The number of bytes in the datagram.</param><param name="hostname">The name of the remote host to which you intend to send the datagram.</param><param name="port">The remote port number with which you intend to communicate.</param><exception cref="T:System.ArgumentNullException"><paramref name="dgram"/> is null. </exception><exception cref="T:System.InvalidOperationException">The <see cref="T:System.Net.Sockets.UdtClient"/> has already established a default remote host. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.UdtClient"/> is closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<int> SendAsync(byte[] datagram, int bytes, string hostname, int port)
        {
            return
                Task<int>.Factory.FromAsync(
                    (Func<AsyncCallback, object, IAsyncResult>)
                        ((callback, state) => this.BeginSend(datagram, bytes, hostname, port, callback, state)),
                    new Func<IAsyncResult, int>(this.EndSend), (object) null);
        }

        /// <summary>
        /// Returns a UDP datagram asynchronously that was sent by a remote host.
        /// </summary>
        /// 
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>.The task object representing the asynchronous operation.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The underlying <see cref="T:System.Net.Sockets.Socket"/>  has been closed. </exception><exception cref="T:System.Net.Sockets.SocketException">An error occurred when accessing the socket. See the Remarks section for more information. </exception>
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<UdpReceiveResult> ReceiveAsync()
        {
            return
                Task<UdpReceiveResult>.Factory.FromAsync(
                    (Func<AsyncCallback, object, IAsyncResult>)
                        ((callback, state) => this.BeginReceive(callback, state)),
                    (Func<IAsyncResult, UdpReceiveResult>) (ar =>
                    {
                        IPEndPoint local_0 = (IPEndPoint) null;
                        return new UdpReceiveResult(this.EndReceive(ar, ref local_0), local_0);
                    }), (object) null);
        }

        private static readonly MethodInfo InternalShutdown = typeof(Socket).GetMethod("InternalShutdown", BindingFlags.NonPublic | BindingFlags.Instance);

        private void FreeResources()
        {
            if (this.m_CleanedUp)
                return;
            Socket client = this.Client;
            if (client != null)
            {
                //Using refection to do this
                //client.InternalShutdown(SocketShutdown.Both);
                InternalShutdown.Invoke(client, new object[] { SocketShutdown.Both });
                
                client.Close();
                this.Client = (Socket) null;
            }
            this.m_CleanedUp = true;
        }

        private bool IsBroadcast(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                return false;
            else
                return ipAddress.Equals(IPAddress.Broadcast);
        }

        private void CheckForBroadcast(IPAddress ipAddress)
        {
            if (this.Client == null || this.m_IsBroadcast || !IsBroadcast(ipAddress))
                return;
            this.m_IsBroadcast = true;
            this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }

        private void createClientSocket()
        {
            this.Client = new Socket(this.m_Family, SocketType.Dgram, ProtocolType.Udp);
        }
    }
}

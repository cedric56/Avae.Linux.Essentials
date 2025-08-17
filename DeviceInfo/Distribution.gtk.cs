namespace Microsoft.Maui.Devices
{
    public readonly struct Distribution : IEquatable<Distribution>
    {
        readonly string distribution;

        /// <summary>
        /// Gets an instance of <see cref="DevicePlatform"/> that represents Android.
        /// </summary>
        public static Distribution Ubuntu { get; } = new Distribution(nameof(Ubuntu));

        public static Distribution Fedora { get; } = new Distribution(nameof(Fedora));
        public static Distribution ArchLinux { get; } = new Distribution(nameof(ArchLinux));
        public static Distribution Debian { get; } = new Distribution(nameof(Debian));
        public static Distribution OpenSUSE { get; } = new Distribution(nameof(OpenSUSE));
        public static Distribution LinuxMint { get; } = new Distribution(nameof(LinuxMint));
        public static Distribution Manjaro { get; } = new Distribution(nameof(Manjaro));
        public static Distribution PopOS { get; } = new Distribution(nameof(PopOS));
        public static Distribution Unknown { get; } = new Distribution(nameof(Unknown));

        internal Distribution(string distribution)
        {
            if (distribution == null)
                throw new ArgumentNullException(nameof(distribution));

            if (distribution.Length == 0)
                throw new ArgumentException(nameof(distribution));

            this.distribution = distribution;
        }

        /// <summary>
        /// Creates a new device platform instance. This can be used to define your custom platforms.
        /// </summary>
        /// <param name="devicePlatform">The device platform identifier.</param>
        /// <returns>A new instance of <see cref="DevicePlatform"/> with the specified platform identifier.</returns>
        public static Distribution Create(string distribution) =>
            new Distribution(distribution);

        /// <summary>
        /// Compares the underlying <see cref="DevicePlatform"/> instances.
        /// </summary>
        /// <param name="other"><see cref="DevicePlatform"/> object to compare with.</param>
        /// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
        public bool Equals(Distribution other) =>
            Equals(other.distribution);

        internal bool Equals(string other) =>
            string.Equals(distribution, other, StringComparison.Ordinal);

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public override bool Equals(object obj) =>
            obj is Distribution && Equals((Distribution)obj);

        /// <summary>
        /// Gets the hash code for this platform instance.
        /// </summary>
        /// <returns>The computed hash code for this device platform or <c>0</c> when the device platform is <see langword="null"/>.</returns>
        public override int GetHashCode() =>
            distribution == null ? 0 : distribution.GetHashCode(
#if !NETSTANDARD2_0
                    StringComparison.Ordinal
#endif
                );

        /// <summary>
        /// Returns a string representation of the current value of <see cref="distribution"/>.
        /// </summary>
        /// <returns>A string representation of this instance in the format of <c>{device platform}</c> or an empty string when no device platform is set.</returns>
        public override string ToString() =>
            distribution ?? string.Empty;

        /// <summary>
        ///	Equality operator for equals.
        /// </summary>
        /// <param name="left">Left to compare.</param>
        /// <param name="right">Right to compare.</param>
        /// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
        public static bool operator ==(Distribution left, Distribution right) =>
            left.Equals(right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left">Left to compare.</param>
        /// <param name="right">Right to compare.</param>
        /// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
        public static bool operator !=(Distribution left, Distribution right) =>
            !left.Equals(right);
    }
}

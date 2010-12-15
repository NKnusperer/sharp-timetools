using System;
using Mono.Unix.Native;
using System.Runtime.InteropServices;

namespace SharpTimetools
{
	public class Linux
	{
		/// <summary>
		/// Set the RTC hardware clock to the given DateTime object. Note that this will not change the Kernel time, see <see cref="SetKernelTime"/>
		/// </summary>
		/// <param name="newTime">
		/// The <see cref="System.DateTime"/> object with the new time
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> that indicates the status of the operation
		/// </returns>
		public static int SetHardwareTime(DateTime newTime)
		{
			rtc_time rtc_tm = new rtc_time();
			rtc_tm.tm_mday = newTime.Day;
			rtc_tm.tm_mon = newTime.Month;
			rtc_tm.tm_year = newTime.Year - 1900;
			rtc_tm.tm_hour = newTime.Hour;
			rtc_tm.tm_min = newTime.Minute;
			rtc_tm.tm_sec = newTime.Second;
			
			int rtc_fd = Syscall.open("/dev/rtc", OpenFlags.O_RDONLY);
			if (rtc_fd == -1)
			{
				return -1;
			}
			int rtc_ioctl = ioctl(rtc_fd, RTC_SET_TIME, ref rtc_tm);
			if (rtc_ioctl == -1)
			{
				return -1;
			}
			Syscall.close(rtc_fd);
			
			return 0;
		}
		
		/// <summary>
		/// Set the kernel time to the given DateTime object. Note that this will not change the hardware RTC clock, see <see cref="SetHardware"/>
		/// </summary>
		/// <param name="newTime">
		/// A <see cref="DateTime"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> that indicates the status of the operation
		/// </returns>
		public static int SetKernelTime(DateTime newTime)
		{
			long unixTime = ToUnixTime(newTime);
			return Syscall.stime(ref unixTime);
		}
		
		/// <summary>
		/// Return the current RTC hardware clock time.
		/// </summary>
		/// <returns>
		/// The <see cref="System.DateTime"/> objekt represent the current RTC time
		/// </returns>
		public static DateTime GetHardwareTime()
		{
			rtc_time rtc_tm = new rtc_time();
			
			int rtc_fd = Syscall.open("/dev/rtc", OpenFlags.O_RDONLY);
			if (rtc_fd == -1)
			{
				return new DateTime();
			}
			int rtc_ioctl = ioctl(rtc_fd, RTC_RD_TIME, ref rtc_tm);
			if (rtc_ioctl == -1)
			{
				return new DateTime();
			}
			Syscall.close(rtc_fd);
			
			return new DateTime(rtc_tm.tm_year + 1900, rtc_tm.tm_mon , rtc_tm.tm_mday, rtc_tm.tm_hour, rtc_tm.tm_min, rtc_tm.tm_sec);
		}
		
		/// <summary>
		/// Return the current kernel time.
		/// You should use <see cref="DateTime.Now"/> instant
		/// </summary>
		/// <returns>
		/// A <see cref="DateTime"/>
		/// </returns>
		[Obsolete("You should use DateTime.Now instant")]
		public static DateTime GetKernelTime()
		{
			return DateTime.Now;
		}
		
		/// <summary>
		/// Sets the RTC time to the time specified by the rtc_time structure pointed to by the third ioctl() argument. To set the RTC time the process must be privileged.
   		/// Defined as a preprocessor macro in /linux/include/linux/rtc.h, line 82.
   		/// This is his most often returned value.
   		/// </summary>>
		private const int RTC_SET_TIME = 1076129802;
		
		/// <summary>
		/// Sets the RTC time to the time specified by the rtc_time structure pointed to by the third ioctl() argument. To set the RTC time the process must be privileged.
   		/// Defined as a preprocessor macro in /linux/include/linux/rtc.h, line 81.
   		/// This is his most often returned value.
   		/// </summary>>
		private const int RTC_RD_TIME = -2145095671;
		
		/// <summary>
		/// Used to pass data via the following ioctl to the kernel.
		/// Defined as a struct in /linux/include/linux/rtc.h, line 20.
		/// </summary>
		private struct rtc_time
		{
			/// <summary>
			/// Seconds after the minute (0-59)
			/// </summary> 
			public int tm_sec;
			/// <summary>
			/// Minutes after the hour	(0-59)
			/// </summary>
			public int tm_min;
			/// <summary>
			/// Hours since midnight (0-23)
			/// </summary>
			public int tm_hour;
			/// <summary>
			/// Day of the month (1-31)
			/// </summary>
			public int tm_mday;
			/// <summary>
			/// Months since January (0-11)
			/// </summary>
			public int tm_mon;
			/// <summary>
			/// Years since 1900
			/// </summary>
			public int tm_year;
			/// <summary>
			/// Days since Sunday (0-6)
			/// </summary>
			[Obsolete("Unused")]
			public int tm_wday;
			/// <summary>
			/// Days since January 1 (0-365)
			/// </summary>
			[Obsolete("Unused")]
			public int tm_yday;
			/// <summary>
			/// Daylight Saving Time flag
			/// </summary>
			[Obsolete("Unused")]
			public int tm_isdst;
		}
		
		/// <summary>
		/// Perform the I/O control operation specified by REQUEST on FD.
   		/// One argument may follow; its presence and type depend on REQUEST.
   		///	Return value depends on REQUEST. Usually -1 indicates error.
		/// Defined in /sys/ioctl.h, line 42.
		/// </summary>
		/// <param name="fd">
		/// File descriptor as returned by <see cref="Mono.Unix.Native.Syscall.open"/>
		/// </param>
		/// <param name="request">
		/// Describe what we want do do, see <see cref="RTC_SET_TIME"/>
		/// </param>
		/// <param name="data">
		/// Date we wont send, see <see cref="rtc_time"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> that indicates the status of a operation
		/// </returns>
		[DllImport("libc.so.6", CharSet = CharSet.Auto)]
		private static extern int ioctl(int fd, int request, ref rtc_time data);
		
		/// <summary>
		/// Converts the given DateTime object to his equivalent Unix timestamp
		/// </summary>
		/// <param name="time">
		/// The <see cref="System.DateTime"/> object to convert
		/// </param>
		/// <returns>
		/// The timestamp as <see cref="long"/>
		/// </returns>
		private static long ToUnixTime(DateTime time)
		{
			TimeSpan ts = time - new DateTime(1970, 1, 1);
			return(Convert.ToInt64(ts.TotalSeconds));
		}
	}
}


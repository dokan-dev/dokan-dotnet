namespace DokanNet
{
    /// <summary>
    /// NT status values.
    /// </summary>
    /// \see <a href="https://msdn.microsoft.com/en-us/library/cc704588.aspx">NTSTATUS Values (MSDN)</a>
    public enum NtStatus : long
    {
        // ***********
        // * Success *
        // ***********

        /// <summary>
        /// Success - The operation completed successfully.
        /// </summary>
        Success = 0x00000000,

        /// <summary>
        /// Success - The caller specified WaitAny for WaitType and one of the dispatcher objects in 
        /// the Object array has been set to the signaled state.
        /// </summary>
        Wait1 = 0x00000001,

        /// <summary>
        /// Success - The caller specified WaitAny for WaitType and one of the dispatcher objects in 
        /// the Object array has been set to the signaled state.
        /// </summary>
        Wait2 = 0x00000002,

        /// <summary>
        /// Success - The caller specified WaitAny for WaitType and one of the dispatcher objects in 
        /// the Object array has been set to the signaled state.
        /// </summary>
        Wait3 = 0x00000003,

        /// <summary>
        /// Success - The caller specified WaitAny for WaitType and one of the dispatcher objects in 
        /// the Object array has been set to the signaled state.
        /// </summary>
        Wait63 = 0x0000003f,

        /// <summary>
        /// Success - The caller attempted to wait for a mutex that has been abandoned.
        /// </summary>
        Abandoned = AbandonedWait0,

        /// <summary>
        /// Success - The caller attempted to wait for a mutex that has been abandoned.
        /// </summary>
        AbandonedWait0 = 0x00000080,

        /// <summary>
        /// Success - The caller attempted to wait for a mutex that has been abandoned.
        /// </summary>
        AbandonedWait1 = 0x00000081,

        /// <summary>
        /// Success - The caller attempted to wait for a mutex that has been abandoned.
        /// </summary>
        AbandonedWait2 = 0x00000082,

        /// <summary>
        /// Success - The caller attempted to wait for a mutex that has been abandoned.
        /// </summary>
        AbandonedWait3 = 0x00000083,

        /// <summary>
        /// Success - The caller attempted to wait for a mutex that has been abandoned.
        /// </summary>
        AbandonedWait63 = 0x000000bf,

        /// <summary>
        /// Success - A user-mode APC was delivered before the given Interval expired.
        /// </summary>
        UserApc = 0x000000c0,

        /// <summary>
        /// Success - ?
        /// </summary>
        KernelApc = 0x00000100,

        /// <summary>
        /// Success - The delay completed because the thread was alerted.
        /// </summary>
        Alerted = 0x00000101,

        /// <summary>
        /// Success - The given Timeout interval expired.
        /// </summary>
        Timeout = 0x00000102,

        /// <summary>
        /// Success - The operation that was requested is pending completion.
        /// </summary>
        Pending = 0x00000103,

        /// <summary>
        /// Success - A reparse should be performed by the Object Manager
        /// because the name of the file resulted in a symbolic link.
        /// </summary>
        Reparse = 0x00000104,

        /// <summary>
        /// Success - Returned by enumeration APIs to indicate more information
        /// is available to successive calls.
        /// </summary>
        MoreEntries = 0x00000105,

        /// <summary>
        /// Success - Indicates not all privileges or groups that are referenced
        /// are assigned to the caller. This allows, for example, all privileges
        /// to be disabled without having to know exactly which privileges are
        /// assigned.
        /// </summary>
        NotAllAssigned = 0x00000106,

        /// <summary>
        /// Success - Some of the information to be translated has not been translated.
        /// </summary>
        SomeNotMapped = 0x00000107,

        /// <summary>
        /// Success - An open/create operation completed while an opportunistic
        /// lock (<c>oplock</c>) break is underway.
        /// </summary>
        OpLockBreakInProgress = 0x00000108,

        /// <summary>
        /// Success - A new volume has been mounted by a file system.
        /// </summary>
        VolumeMounted = 0x00000109,

        /// <summary>
        /// Success - This success level status indicates that the transaction
        /// state already exists for the registry sub-tree but that a transaction
        /// commit was previously aborted. The commit has now been completed.
        /// </summary>
        RxActCommitted = 0x0000010a,

        /// <summary>
        /// Success - Indicates that a notify change request has been completed
        /// due to closing the handle that made the notify change request.
        /// </summary>
        NotifyCleanup = 0x0000010b,

        /// <summary>
        /// Success - Indicates that a notify change request is being completed
        /// and that the information is not being returned in the caller's
        /// buffer. The caller now needs to enumerate the files to find the
        /// changes.
        /// </summary>
        NotifyEnumDir = 0x0000010c,

        /// <summary>
        /// Success - {No Quotas} No system quota limits are specifically set for this account.
        /// </summary>
        NoQuotasForAccount = 0x0000010d,

        /// <summary>
        /// Success - {Connect Failure on Primary Transport} An attempt was made
        /// to connect to the remote server on the primary transport, but
        /// the connection failed. The computer WAS able to connect on a
        /// secondary transport.
        /// </summary>
        PrimaryTransportConnectFailed = 0x0000010e,

        /// <summary>
        /// Success - The page fault was a transition fault.
        /// </summary>
        PageFaultTransition = 0x00000110,

        /// <summary>
        /// Success - The page fault was a demand zero fault.
        /// </summary>
        PageFaultDemandZero = 0x00000111,

        /// <summary>
        /// Success - The page fault was a demand zero fault.
        /// </summary>
        PageFaultCopyOnWrite = 0x00000112,

        /// <summary>
        /// Success - The page fault was a demand zero fault.
        /// </summary>
        PageFaultGuardPage = 0x00000113,

        /// <summary>
        /// Success - The page fault was satisfied by reading from a secondary
        /// storage device.
        /// </summary>
        PageFaultPagingFile = 0x00000114,

        /// <summary>
        /// Success - The crash dump exists in a paging file.
        /// </summary>
        CrashDump = 0x00000116,

        /// <summary>
        /// Success - A reparse should be performed by the Object Manager
        /// because the name of the file resulted in a symbolic link.
        /// </summary>
        ReparseObject = 0x00000118,

        /// <summary>
        /// Success - A process being terminated has no threads to terminate.
        /// </summary>
        NothingToTerminate = 0x00000122,

        /// <summary>
        /// Success - The specified process is not part of a job.
        /// </summary>
        ProcessNotInJob = 0x00000123,

        /// <summary>
        /// Success - The specified process is part of a job.
        /// </summary>
        ProcessInJob = 0x00000124,

        /// <summary>
        /// Success - The current process is a cloned process.
        /// </summary>
        ProcessCloned = 0x00000129,

        /// <summary>
        /// Success - The file was locked and all users of the file can only read.
        /// </summary>
        FileLockedWithOnlyReaders = 0x0000012a,

        /// <summary>
        /// Success - The file was locked and at least one user of the file can write.
        /// </summary>
        FileLockedWithWriters = 0x0000012b,
       
        // *****************
        // * Informational *
        // *****************

        /// <summary>
        /// Informational - General information
        /// </summary>
        Informational = 0x40000000,

        /// <summary>
        /// Informational - {Object Exists} An attempt was made to create an object but 
        /// the object name already exists.
        /// </summary>
        ObjectNameExists = 0x40000000,

        /// <summary>
        /// Informational - {Thread Suspended} A thread termination occurred while 
        /// the thread was suspended. The thread resumed, and termination proceeded.
        /// </summary>
        ThreadWasSuspended = 0x40000001,

        /// <summary>
        /// Informational - {Working Set Range Error} An attempt was made to set the working set 
        /// minimum or maximum to values that are outside the allowable range.
        /// </summary>
        WorkingSetLimitRange = 0x40000002,

        /// <summary>
        /// Informational - {Image Relocated} An image file could not be mapped at the address 
        /// that is specified in the image file. Local fixes must be performed on this image.
        /// </summary>
        ImageNotAtBase = 0x40000003,

        /// <summary>
        /// Informational - {Registry Recovery} One of the files that contains the system 
        /// registry data had to be recovered by using a log or alternate copy. 
        /// The recovery was successful.
        /// </summary>
        RegistryRecovered = 0x40000009,

        /// <summary>
        /// Informational - Transaction - The transactional resource manager is already consistent.
        /// Recovery is not needed.
        /// </summary>
        RecoveryNotNeeded = 0x40190034,

        /// <summary>
        /// Informational - Transaction - The transactional resource manager has
        /// already been started.
        /// </summary>
        RmAlreadyStarted = 0x40190035,

        // ***********
        // * Warning *
        // ***********

        /// <summary>
        /// Warning - General warning
        /// </summary>
        Warning = 0x80000000,

        /// <summary>
        /// Warning - {EXCEPTION} Guard Page Exception A page of memory that marks 
        /// the end of a data structure, such as a stack or an array, 
        /// has been accessed.
        /// </summary>
        GuardPageViolation = 0x80000001,

        /// <summary>
        /// Warning - {EXCEPTION} Alignment Fault A data type misalignment was detected 
        /// in a load or store instruction.
        /// </summary>
        DatatypeMisalignment = 0x80000002,

        /// <summary>
        /// Warning - {EXCEPTION} Breakpoint A breakpoint has been reached.
        /// </summary>
        Breakpoint = 0x80000003,

        /// <summary>
        /// Warning - {EXCEPTION} Single Step A single step or trace operation has just been completed.
        /// </summary>
        SingleStep = 0x80000004,

        /// <summary>
        /// Warning - {Buffer Overflow} The data was too large to fit into the specified buffer.
        /// </summary>
        BufferOverflow = 0x80000005,

        /// <summary>
        /// Warning - {No More Files} No more files were found which match the file specification.
        /// </summary>
        NoMoreFiles = 0x80000006,

        /// <summary>
        /// Warning - {Handles Closed} Handles to objects have been automatically closed 
        /// because of the requested operation.
        /// </summary>
        HandlesClosed = 0x8000000a,

        /// <summary>
        /// Warning - Because of protection conflicts, not all the requested bytes could be copied.
        /// </summary>
        PartialCopy = 0x8000000d,

        /// <summary>
        /// Warning - {Device Busy} The device is currently busy.
        /// </summary>
        DeviceBusy = 0x80000011,

        /// <summary>
        /// Warning - {Illegal EA} The specified extended attribute (EA) name
        /// contains at least one illegal character.
        /// </summary>
        InvalidEaName = 0x80000013,

        /// <summary>
        /// Warning - {Inconsistent EA List} The extended attribute (EA) list is
        /// inconsistent.
        /// </summary>
        EaListInconsistent = 0x80000014,

        /// <summary>
        /// Warning - {No More Entries} No more entries are available from an
        /// enumeration operation.
        /// </summary>
        NoMoreEntries = 0x8000001a,

        /// <summary>
        /// Warning - A long jump has been executed.
        /// </summary>
        LongJump = 0x80000026,

        /// <summary>
        /// Warning - The application is attempting to run executable code. 
        /// This may be insecure. 
        /// </summary>
        DllMightBeInsecure = 0x8000002b,

        /// <summary>
        /// Warning - Transaction - There is no transaction metadata on the file.
        /// </summary>
        LogCorruptionDetected = 0x80190029,

        /// <summary>
        /// Warning - Transaction - The file cannot be recovered because there
        /// is a handle still open on it.
        /// </summary>
        CantRecoverWithHandleOpen = 0x80190031,

        /// <summary>
        /// Warning - Transaction - Transaction metadata is already present on
        /// this file and cannot be superseded.
        /// </summary>
        TxfMetadataAlreadyPresent = 0x80190041,

        /// <summary>
        /// Warning - Transaction - A transaction scope could not be entered
        /// because the scope handler has not been initialized.
        /// </summary>
        TransactionScopeCallbacksNotSet = 0x80190042,

        // *********
        // * Error *
        // *********

        /// <summary>
        /// Error - General error
        /// </summary>
        Error = 0xc0000000,

        /// <summary>
        /// Error - {Operation Failed} The requested operation was unsuccessful.
        /// </summary>
        Unsuccessful = 0xc0000001,

        /// <summary>
        /// Error - {Not Implemented} The requested operation is not implemented.
        /// </summary>
        NotImplemented = 0xc0000002,

        /// <summary>
        /// Error - {Invalid Parameter} The specified information class is not a 
        /// valid information class for the specified object.
        /// </summary>
        InvalidInfoClass = 0xc0000003,

        /// <summary>
        /// Error - The specified information record length does not match the
        /// length that is required for the specified information class.
        /// </summary>
        InfoLengthMismatch = 0xc0000004,

        /// <summary>
        /// Error - The instruction referenced memory it do not have access to.
        /// </summary>
        AccessViolation = 0xc0000005,

        /// <summary>
        /// Error - The required data was not placed into memory because of an
        /// I/O error status.
        /// </summary>
        InPageError = 0xc0000006,

        /// <summary>
        /// Error - The page file quota for the process has been exhausted.
        /// </summary>
        PagefileQuota = 0xc0000007,

        /// <summary>
        /// Error - An invalid HANDLE was specified.
        /// </summary>
        InvalidHandle = 0xc0000008,

        /// <summary>
        /// Error - An invalid initial stack was specified in a call to
        /// <c>NtCreateThread</c>.
        /// </summary>
        BadInitialStack = 0xc0000009,

        /// <summary>
        /// Error - An invalid initial start address was specified in a call to
        /// <c>NtCreateThread</c>.
        /// </summary>
        BadInitialPc = 0xc000000a,

        /// <summary>
        /// Error - An invalid client ID was specified.
        /// </summary>
        InvalidCid = 0xc000000b,

        /// <summary>
        /// Error - An attempt was made to cancel or set a timer that has an
        /// associated APC and the specified thread is not the thread that
        /// originally set the timer with an associated APC routine.
        /// </summary>
        TimerNotCanceled = 0xc000000c,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function.
        /// </summary>
        InvalidParameter = 0xc000000d,

        /// <summary>
        /// Error - A device that does not exist was specified.
        /// </summary>
        NoSuchDevice = 0xc000000e,

        /// <summary>
        /// Error - {File Not Found} The file does not exist.
        /// </summary>
        NoSuchFile = 0xc000000f,

        /// <summary>
        /// Error - The specified request is not a valid operation for the
        /// target device.
        /// </summary>
        InvalidDeviceRequest = 0xc0000010,

        /// <summary>
        /// Error - The end-of-file marker has been reached. There is no valid
        /// data in the file beyond this marker.
        /// </summary>
        EndOfFile = 0xc0000011,

        /// <summary>
        /// Error - {Wrong Volume} The wrong volume is in the drive. Insert
        /// volume into drive.
        /// </summary>
        WrongVolume = 0xc0000012,

        /// <summary>
        /// Error - {No Disk} There is no disk in the drive. Insert a disk into
        /// drive.
        /// </summary>
        NoMediaInDevice = 0xc0000013,

        /// <summary>
        /// Error - {Not Enough Quota} Not enough virtual memory or paging file
        /// quota is available to complete the specified operation.
        /// </summary>
        NoMemory = 0xc0000017,

        /// <summary>
        /// Error - The address range to un-map is not a mapped view.
        /// </summary>
        NotMappedView = 0xc0000019,

        /// <summary>
        /// Error - The virtual memory cannot be freed.
        /// </summary>
        UnableToFreeVm = 0xc000001a,

        /// <summary>
        /// Error - The specified section cannot be deleted.
        /// </summary>
        UnableToDeleteSection = 0xc000001b,

        /// <summary>
        /// Error - {EXCEPTION} Illegal Instruction. An attempt was made to
        /// execute an illegal instruction.
        /// </summary>
        IllegalInstruction = 0xc000001d,

        /// <summary>
        /// Error - {Already Committed} The specified address range is already
        /// committed.
        /// </summary>
        AlreadyCommitted = 0xc0000021,

        /// <summary>
        /// Error - {Access Denied} A process has requested access to an object
        /// but has not been granted those access rights.
        /// </summary>
        AccessDenied = 0xc0000022,

        /// <summary>
        /// Error - {Buffer Too Small} The buffer is too small to contain the
        /// entry. No information has been written to the buffer.
        /// </summary>
        BufferTooSmall = 0xc0000023,

        /// <summary>
        /// Error - {Wrong Type} There is a mismatch between the type of object
        /// that is required by the requested operation and the type of object
        /// that is specified in the request.
        /// </summary>
        ObjectTypeMismatch = 0xc0000024,

        /// <summary>
        /// Error - {EXCEPTION} Cannot Continue. Windows cannot continue from
        /// this exception.
        /// </summary>
        NonContinuableException = 0xc0000025,

        /// <summary>
        /// Error - An invalid or unaligned stack was encountered during an
        /// unwind operation.
        /// </summary>
        BadStack = 0xc0000028,

        /// <summary>
        /// Error - An attempt was made to unlock a page of memory that was not
        /// locked.
        /// </summary>
        NotLocked = 0xc000002a,

        /// <summary>
        /// Error - An attempt was made to change the attributes on memory that
        /// has not been committed.
        /// </summary>
        NotCommitted = 0xc000002d,

        /// <summary>
        /// Error - An invalid combination of parameters was specified.
        /// </summary>
        InvalidParameterMix = 0xc0000030,

        /// <summary>
        /// Error - The object name is invalid.
        /// </summary>
        ObjectNameInvalid = 0xc0000033,

        /// <summary>
        /// Error - The object name is not found.
        /// </summary>
        ObjectNameNotFound = 0xc0000034,

        /// <summary>
        /// Error - The object name already exists.
        /// </summary>
        ObjectNameCollision = 0xc0000035,

        /// <summary>
        /// Error - The object path component was not a directory object.
        /// </summary>
        ObjectPathInvalid = 0xc0000039,

        /// <summary>
        /// Error - {Path Not Found} The path does not exist.
        /// </summary>
        ObjectPathNotFound = 0xc000003a,

        /// <summary>
        /// Error - The object path component was not a directory object.
        /// </summary>
        ObjectPathSyntaxBad = 0xc000003b,

        /// <summary>
        /// Error - {Data Overrun} A data overrun error occurred.
        /// </summary>
        DataOverrun = 0xc000003c,

        /// <summary>
        /// Error - {Data Late} A data late error occurred.
        /// </summary>
        DataLate = 0xc000003d,

        /// <summary>
        /// Error - {Data Error} An error occurred in reading or writing data.
        /// </summary>
        DataError = 0xc000003e,

        /// <summary>
        /// Error - {Bad CRC} A cyclic redundancy check (CRC) checksum error
        /// occurred.
        /// </summary>
        CrcError = 0xc000003f,

        /// <summary>
        /// Error - {Section Too Large} The specified section is too big to map
        /// the file.
        /// </summary>
        SectionTooBig = 0xc0000040,

        /// <summary>
        /// Error - The <c>NtConnectPort</c> request is refused.
        /// </summary>
        PortConnectionRefused = 0xc0000041,

        /// <summary>
        /// Error - The type of port handle is invalid for the operation that is
        /// requested.
        /// </summary>
        InvalidPortHandle = 0xc0000042,

        /// <summary>
        /// Error - A file cannot be opened because the share access flags are
        /// incompatible.
        /// </summary>
        SharingViolation = 0xc0000043,

        /// <summary>
        /// Error - Insufficient quota exists to complete the operation.
        /// </summary>
        QuotaExceeded = 0xc0000044,

        /// <summary>
        /// Error - The specified page protection was not valid.
        /// </summary>
        InvalidPageProtection = 0xc0000045,

        /// <summary>
        /// Error - An attempt to release a mutant object was made by a thread
        /// that was not the owner of the mutant object.
        /// </summary>
        MutantNotOwned = 0xc0000046,

        /// <summary>
        /// Error - An attempt was made to release a semaphore such that its
        /// maximum count would have been exceeded.
        /// </summary>
        SemaphoreLimitExceeded = 0xc0000047,

        /// <summary>
        /// Error - An attempt was made to set the DebugPort or ExceptionPort of
        /// a process, but a port already exists in the process, or an attempt
        /// was made to set the CompletionPort of a file but a port was already
        /// set in the file, or an attempt was made to set the associated
        /// completion port of an ALPC port but it is already set.
        /// </summary>
        PortAlreadySet = 0xc0000048,

        /// <summary>
        /// Error - An attempt was made to query image information on a section
        /// that does not map an image.
        /// </summary>
        SectionNotImage = 0xc0000049,

        /// <summary>
        /// Error - An attempt was made to suspend a thread whose suspend count
        /// was at its maximum.
        /// </summary>
        SuspendCountExceeded = 0xc000004a,

        /// <summary>
        /// Error - An attempt was made to suspend a thread that has begun
        /// termination.
        /// </summary>
        ThreadIsTerminating = 0xc000004b,

        /// <summary>
        /// Error - An attempt was made to set the working set limit to an
        /// invalid value (for example, the minimum greater than maximum).
        /// </summary>
        BadWorkingSetLimit = 0xc000004c,

        /// <summary>
        /// Error - A section was created to map a file that is not compatible
        /// with an already existing section that maps the same file.
        /// </summary>
        IncompatibleFileMap = 0xc000004d,

        /// <summary>
        /// Error - A view to a section specifies a protection that is
        /// incompatible with the protection of the initial view.
        /// </summary>
        SectionProtection = 0xc000004e,

        /// <summary>
        /// Error - An operation involving EAs failed because the file system
        /// does not support EAs.
        /// </summary>
        EasNotSupported = 0xc000004f,

        /// <summary>
        /// Error - An EA operation failed because the EA set is too large.
        /// </summary>
        EaTooLarge = 0xc0000050,

        /// <summary>
        /// Error - An EA operation failed because the name or EA index is
        /// invalid.
        /// </summary>
        NonExistentEaEntry = 0xc0000051,

        /// <summary>
        /// Error - The file for which EAs were requested has no EAs.
        /// </summary>
        NoEasOnFile = 0xc0000052,

        /// <summary>
        /// Error - The EA is corrupt and cannot be read.
        /// </summary>
        EaCorruptError = 0xc0000053,

        /// <summary>
        /// Error - A requested read/write cannot be granted due to a
        /// conflicting file lock.
        /// </summary>
        FileLockConflict = 0xc0000054,

        /// <summary>
        /// Error - A requested file lock cannot be granted due to other
        /// existing locks.
        /// </summary>
        LockNotGranted = 0xc0000055,

        /// <summary>
        /// Error - A non-close operation has been requested of a file object
        /// that has a delete pending.
        /// </summary>
        DeletePending = 0xc0000056,

        /// <summary>
        /// Error - An attempt was made to set the control attribute on a file.
        /// This attribute is not supported in the destination file system.
        /// </summary>
        CtlFileNotSupported = 0xc0000057,

        /// <summary>
        /// Error - Indicates a revision number that was encountered or
        /// specified is not one that is known by the service. It might be a
        /// more recent revision than the service is aware of.
        /// </summary>
        UnknownRevision = 0xc0000058,

        /// <summary>
        /// Error - Indicates that two revision levels are incompatible.
        /// </summary>
        RevisionMismatch = 0xc0000059,

        /// <summary>
        /// Error - Indicates a particular security ID cannot be assigned as
        /// the owner of an object.
        /// </summary>
        InvalidOwner = 0xc000005a,

        /// <summary>
        /// Error - Indicates a particular security ID cannot be assigned as the
        /// primary group of an object.
        /// </summary>
        InvalidPrimaryGroup = 0xc000005b,

        /// <summary>
        /// Error - An attempt has been made to operate on an impersonation
        /// token by a thread that is not currently impersonating a client.
        /// </summary>
        NoImpersonationToken = 0xc000005c,

        /// <summary>
        /// Error - A mandatory group cannot be disabled.
        /// </summary>
        CantDisableMandatory = 0xc000005d,

        /// <summary>
        /// Error - No logon servers are currently available to service the
        /// logon request.
        /// </summary>
        NoLogonServers = 0xc000005e,

        /// <summary>
        /// Error - A specified logon session does not exist. It might already
        /// have been terminated.
        /// </summary>
        NoSuchLogonSession = 0xc000005f,

        /// <summary>
        /// Error - A specified privilege does not exist.
        /// </summary>
        NoSuchPrivilege = 0xc0000060,

        /// <summary>
        /// Error - A required privilege is not held by the client.
        /// </summary>
        PrivilegeNotHeld = 0xc0000061,

        /// <summary>
        /// Error - The name provided is not a properly formed account name.
        /// </summary>
        InvalidAccountName = 0xc0000062,

        /// <summary>
        /// Error - The specified account already exists.
        /// </summary>
        UserExists = 0xc0000063,

        /// <summary>
        /// Error - The specified account does not exist.
        /// </summary>
        NoSuchUser = 0xc0000064,

        /// <summary>
        /// Error - The specified group already exists.
        /// </summary>
        GroupExists = 0xc0000065,

        /// <summary>
        /// Error - The specified group does not exist.
        /// </summary>
        NoSuchGroup = 0xc0000066,

        /// <summary>
        /// Error - The specified user account is already in the specified group
        /// account. Also used to indicate a group cannot be deleted because it
        /// contains a member.
        /// </summary>
        MemberInGroup = 0xc0000067,

        /// <summary>
        /// Error - The specified user account is not a member of the specified
        /// group account.
        /// </summary>
        MemberNotInGroup = 0xc0000068,

        /// <summary>
        /// Error - Indicates the requested operation would disable or delete
        /// the last remaining administration account. This is not allowed to
        /// prevent creating a situation in which the system cannot be
        /// administrated.
        /// </summary>
        LastAdmin = 0xc0000069,

        /// <summary>
        /// Error - When trying to update a password, this return status
        /// indicates that the value provided as the current password is not
        /// correct.
        /// </summary>
        WrongPassword = 0xc000006a,

        /// <summary>
        /// Error - When trying to update a password, this return status
        /// indicates that the value provided for the new password contains
        /// values that are not allowed in passwords.
        /// </summary>
        IllFormedPassword = 0xc000006b,

        /// <summary>
        /// Error - When trying to update a password, this status indicates that
        /// some password update rule has been violated. For example, the
        /// password might not meet length criteria.
        /// </summary>
        PasswordRestriction = 0xc000006c,

        /// <summary>
        /// Error - The attempted logon is invalid. This is either due to a bad
        /// username or authentication information.
        /// </summary>
        LogonFailure = 0xc000006d,

        /// <summary>
        /// Error - Indicates a referenced user name and authentication
        /// information are valid, but some user account restriction has
        /// prevented successful authentication (such as time-of-day
        /// restrictions).
        /// </summary>
        AccountRestriction = 0xc000006e,

        /// <summary>
        /// Error - The user account has time restrictions and cannot be logged
        /// onto at this time.
        /// </summary>
        InvalidLogonHours = 0xc000006f,

        /// <summary>
        /// Error - The user account is restricted so that it cannot be used to
        /// log on from the source workstation.
        /// </summary>
        InvalidWorkstation = 0xc0000070,

        /// <summary>
        /// Error - The user account password has expired.
        /// </summary>
        PasswordExpired = 0xc0000071,

        /// <summary>
        /// Error - The referenced account is currently disabled and cannot be
        /// logged on to.
        /// </summary>
        AccountDisabled = 0xc0000072,

        /// <summary>
        /// Error - None of the information to be translated has been
        /// translated.
        /// </summary>
        NoneMapped = 0xc0000073,

        /// <summary>
        /// Error - The number of LUIDs requested cannot be allocated with a
        /// single allocation.
        /// </summary>
        TooManyLuidsRequested = 0xc0000074,

        /// <summary>
        /// Error - Indicates there are no more LUIDs to allocate.
        /// </summary>
        LuidsExhausted = 0xc0000075,

        /// <summary>
        /// Error - Indicates the sub-authority value is invalid for the
        /// particular use.
        /// </summary>
        InvalidSubAuthority = 0xc0000076,

        /// <summary>
        /// Error - Indicates the ACL structure is not valid.
        /// </summary>
        InvalidAcl = 0xc0000077,

        /// <summary>
        /// Error - Indicates the SID structure is not valid.
        /// </summary>
        InvalidSid = 0xc0000078,

        /// <summary>
        /// Error - Indicates the <c>SECURITY_DESCRIPTOR</c> structure is not valid.
        /// </summary>
        InvalidSecurityDescr = 0xc0000079,

        /// <summary>
        /// Error - Indicates the specified procedure address cannot be found in
        /// the DLL.
        /// </summary>
        ProcedureNotFound = 0xc000007a,

        /// <summary>
        /// Error - {Bad Image} Image is either not designed to run on Windows or
        /// it contains an error. Try installing the program again using the
        /// original installation media or contact your system administrator or
        /// the software vendor for support.
        /// </summary>
        InvalidImageFormat = 0xc000007b,

        /// <summary>
        /// Error - An attempt was made to reference a token that does not
        /// exist. This is typically done by referencing the token that is
        /// associated with a thread when the thread is not impersonating a
        /// client.
        /// </summary>
        NoToken = 0xc000007c,

        /// <summary>
        /// Error - Indicates that an attempt to build either an inherited ACL
        /// or ACE was not successful. This can be caused by a number of things.
        /// One of the more probable causes is the replacement of a CreatorId
        /// with a SID that did not fit into the ACE or ACL.
        /// </summary>
        BadInheritanceAcl = 0xc000007d,

        /// <summary>
        /// Error - The range specified in <c>NtUnlockFile</c> was not locked.
        /// </summary>
        RangeNotLocked = 0xc000007e,

        /// <summary>
        /// Error - An operation failed because the disk was full.
        /// </summary>
        DiskFull = 0xc000007f,

        /// <summary>
        /// Error - The GUID allocation server is disabled at the moment.
        /// </summary>
        ServerDisabled = 0xc0000080,

        /// <summary>
        /// Error - The GUID allocation server is enabled at the moment.
        /// </summary>
        ServerNotDisabled = 0xc0000081,

        /// <summary>
        /// Error - Too many GUIDs were requested from the allocation server at
        /// once.
        /// </summary>
        TooManyGuidsRequested = 0xc0000082,

        /// <summary>
        /// Error - The GUIDs could not be allocated because the Authority Agent
        /// was exhausted.
        /// </summary>
        GuidsExhausted = 0xc0000083,

        /// <summary>
        /// Error - The value provided was an invalid value for an identifier
        /// authority.
        /// </summary>
        InvalidIdAuthority = 0xc0000084,

        /// <summary>
        /// Error - No more authority agent values are available for the
        /// particular identifier authority value.
        /// </summary>
        AgentsExhausted = 0xc0000085,

        /// <summary>
        /// Error - An invalid volume label has been specified.
        /// </summary>
        InvalidVolumeLabel = 0xc0000086,

        /// <summary>
        /// Error - A mapped section could not be extended.
        /// </summary>
        SectionNotExtended = 0xc0000087,

        /// <summary>
        /// Error - Specified section to flush does not map a data file.
        /// </summary>
        NotMappedData = 0xc0000088,

        /// <summary>
        /// Error - Indicates the specified image file did not contain a
        /// resource section.
        /// </summary>
        ResourceDataNotFound = 0xc0000089,

        /// <summary>
        /// Error - Indicates the specified resource type cannot be found in the
        /// image file.
        /// </summary>
        ResourceTypeNotFound = 0xc000008a,

        /// <summary>
        /// Error - Indicates the specified resource name cannot be found in the
        /// image file.
        /// </summary>
        ResourceNameNotFound = 0xc000008b,

        /// <summary>
        /// Error - {EXCEPTION} Array bounds exceeded.
        /// </summary>
        ArrayBoundsExceeded = 0xc000008c,

        /// <summary>
        /// Error - {EXCEPTION} Floating-point denormal operand.
        /// </summary>
        FloatDenormalOperand = 0xc000008d,

        /// <summary>
        /// Error - {EXCEPTION} Floating-point division by zero.
        /// </summary>
        FloatDivideByZero = 0xc000008e,

        /// <summary>
        /// Error - {EXCEPTION} Floating-point inexact result.
        /// </summary>
        FloatInexactResult = 0xc000008f,

        /// <summary>
        /// Error - {EXCEPTION} Floating-point invalid operation.
        /// </summary>
        FloatInvalidOperation = 0xc0000090,

        /// <summary>
        /// Error - {EXCEPTION} Floating-point overflow.
        /// </summary>
        FloatOverflow = 0xc0000091,

        /// <summary>
        /// Error - {EXCEPTION} Floating-point stack check.
        /// </summary>
        FloatStackCheck = 0xc0000092,

        /// <summary>
        /// Error - {EXCEPTION} Floating-point underflow.
        /// </summary>
        FloatUnderflow = 0xc0000093,

        /// <summary>
        /// Error - {EXCEPTION} Integer division by zero.
        /// </summary>
        IntegerDivideByZero = 0xc0000094,

        /// <summary>
        /// Error - {EXCEPTION} Integer overflow.
        /// </summary>
        IntegerOverflow = 0xc0000095,

        /// <summary>
        /// Error - {EXCEPTION} Privileged instruction.
        /// </summary>
        PrivilegedInstruction = 0xc0000096,

        /// <summary>
        /// Error - An attempt was made to install more paging files than the
        /// system supports.
        /// </summary>
        TooManyPagingFiles = 0xc0000097,

        /// <summary>
        /// Error - The volume for a file has been externally altered such that
        /// the opened file is no longer valid.
        /// </summary>
        FileInvalid = 0xc0000098,

        /// <summary>
        /// Error - The maximum named pipe instance count has been reached.
        /// </summary>
        InstanceNotAvailable = 0xc00000ab,

        /// <summary>
        /// Error - An instance of a named pipe cannot be found in the listening
        /// state.
        /// </summary>
        PipeNotAvailable = 0xc00000ac,

        /// <summary>
        /// Error - The named pipe is not in the connected or closing state.
        /// </summary>
        InvalidPipeState = 0xc00000ad,

        /// <summary>
        /// Error - The specified pipe is set to complete operations and there
        /// are current I/O operations queued so that it cannot be changed to
        /// queue operations.
        /// </summary>
        PipeBusy = 0xc00000ae,

        /// <summary>
        /// Error - The specified handle is not open to the server end of the
        /// named pipe.
        /// </summary>
        IllegalFunction = 0xc00000af,

        /// <summary>
        /// Error - The specified named pipe is in the disconnected state.
        /// </summary>
        PipeDisconnected = 0xc00000b0,

        /// <summary>
        /// Error - The specified named pipe is in the closing state.
        /// </summary>
        PipeClosing = 0xc00000b1,

        /// <summary>
        /// Error - The specified named pipe is in the connected state.
        /// </summary>
        PipeConnected = 0xc00000b2,

        /// <summary>
        /// Error - The specified named pipe is in the listening state.
        /// </summary>
        PipeListening = 0xc00000b3,

        /// <summary>
        /// Error - The specified named pipe is not in message mode.
        /// </summary>
        InvalidReadMode = 0xc00000b4,

        /// <summary>
        /// Error - {Device Timeout} The specified I/O operation was not
        /// completed before the time-out period expired.
        /// </summary>
        IoTimeout = 0xc00000b5,

        /// <summary>
        /// Error - The specified file has been closed by another process.
        /// </summary>
        FileForcedClosed = 0xc00000b6,

        /// <summary>
        /// Error - Profiling is not started.
        /// </summary>
        ProfilingNotStarted = 0xc00000b7,

        /// <summary>
        /// Error - Profiling is not stopped.
        /// </summary>
        ProfilingNotStopped = 0xc00000b8,

        /// <summary>
        /// Error - {Incorrect Volume} The destination file of a rename request
        /// is located on a different device than the source of the rename
        /// request.
        /// </summary>
        NotSameDevice = 0xc00000d4,

        /// <summary>
        /// Error - The specified file has been renamed and thus cannot be
        /// modified.
        /// </summary>
        FileRenamed = 0xc00000d5,

        /// <summary>
        /// Error - Used to indicate that an operation cannot continue without
        /// blocking for I/O.
        /// </summary>
        CantWait = 0xc00000d8,

        /// <summary>
        /// Error - Used to indicate that a read operation was done on an empty
        /// pipe.
        /// </summary>
        PipeEmpty = 0xc00000d9,

        /// <summary>
        /// Error - Indicates that a thread attempted to terminate itself by
        /// default (called <c>NtTerminateThread</c> with <c>NULL</c>) and it was the last
        /// thread in the current process.
        /// </summary>
        CantTerminateSelf = 0xc00000db,

        /// <summary>
        /// Error - An internal error occurred.
        /// </summary>
        InternalError = 0xc00000e5,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the first argument.
        /// </summary>
        InvalidParameter1 = 0xc00000ef,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the second argument.
        /// </summary>
        InvalidParameter2 = 0xc00000f0,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the third argument.
        /// </summary>
        InvalidParameter3 = 0xc00000f1,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the fourth argument.
        /// </summary>
        InvalidParameter4 = 0xc00000f2,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the fifth argument.
        /// </summary>
        InvalidParameter5 = 0xc00000f3,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the sixth argument.
        /// </summary>
        InvalidParameter6 = 0xc00000f4,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the seventh argument.
        /// </summary>
        InvalidParameter7 = 0xc00000f5,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the eighth argument.
        /// </summary>
        InvalidParameter8 = 0xc00000f6,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the ninth argument.
        /// </summary>
        InvalidParameter9 = 0xc00000f7,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the tenth argument.
        /// </summary>
        InvalidParameter10 = 0xc00000f8,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the eleventh argument.
        /// </summary>
        InvalidParameter11 = 0xc00000f9,

        /// <summary>
        /// Error - An invalid parameter was passed to a service or function as
        /// the twelfth argument.
        /// </summary>
        InvalidParameter12 = 0xc00000fa,

        /// <summary>
        /// Error - Indicates that the directory trying to be deleted is not empty.
        /// </summary>
        DirectoryNotEmpty = 0xc0000101,

        /// <summary>
        /// Error - A requested opened file is not a directory.
        /// </summary>
        NotADirectory = 0xc0000103,

        /// <summary>
        /// Error - An attempt was made to map a file of size zero with the
        /// maximum size specified as zero.
        /// </summary>
        MappedFileSizeZero = 0xc000011e,

        /// <summary>
        /// Error - Too many files are opened on a remote server. This error
        /// should only be returned by the Windows redirector on a remote drive.
        /// </summary>
        TooManyOpenedFiles = 0xc000011f,

        /// <summary>
        /// Error - The I/O request was canceled.
        /// </summary>
        Cancelled = 0xc0000120,

        /// <summary>
        /// Error - An attempt has been made to remove a file or directory that
        /// cannot be deleted.
        /// </summary>
        CannotDelete = 0xc0000121,

        /// <summary>
        /// Error - Indicates a name that was specified as a remote computer
        /// name is syntactically invalid.
        /// </summary>
        InvalidComputerName = 0xc0000122,

        /// <summary>
        /// Error - An I/O request other than <c>close</c> was performed on a file
        /// after it was deleted, which can only happen to a request that did
        /// not complete before the last handle was closed via <c>NtClose</c>.
        /// </summary>
        FileDeleted = 0xc0000123,

        /// <summary>
        /// Error - Indicates an operation that is incompatible with built-in
        /// accounts has been attempted on a built-in (special) SAM account. For
        /// example, built-in accounts cannot be deleted.
        /// </summary>
        SpecialAccount = 0xc0000124,

        /// <summary>
        /// Error - The operation requested cannot be performed on the specified
        /// group because it is a built-in special group.
        /// </summary>
        SpecialGroup = 0xc0000125,

        /// <summary>
        /// Error - The operation requested cannot be performed on the specified
        /// user because it is a built-in special user.
        /// </summary>
        SpecialUser = 0xc0000126,

        /// <summary>
        /// Error - Indicates a member cannot be removed from a group because
        /// the group is currently the member's primary group.
        /// </summary>
        MembersPrimaryGroup = 0xc0000127,

        /// <summary>
        /// Error - An I/O request other than <c>close</c> and several other special
        /// case operations was attempted using a file object that had already
        /// been closed.
        /// </summary>
        FileClosed = 0xc0000128,

        /// <summary>
        /// Error - Indicates a process has too many threads to perform the
        /// requested action. For example, assignment of a primary token can be
        /// performed only when a process has zero or one threads.
        /// </summary>
        TooManyThreads = 0xc0000129,

        /// <summary>
        /// Error - An attempt was made to operate on a thread within a specific
        /// process, but the specified thread is not in the specified process.
        /// </summary>
        ThreadNotInProcess = 0xc000012a,

        /// <summary>
        /// Error - An attempt was made to establish a token for use as a
        /// primary token but the token is already in use. A token can only be
        /// the primary token of one process at a time.
        /// </summary>
        TokenAlreadyInUse = 0xc000012b,

        /// <summary>
        /// Error - The page file quota was exceeded.
        /// </summary>
        PagefileQuotaExceeded = 0xc000012c,

        /// <summary>
        /// Error - {Out of Virtual Memory} Your system is low on virtual
        /// memory. To ensure that Windows runs correctly, increase the size of
        /// your virtual memory paging file.
        /// </summary>
        CommitmentLimit = 0xc000012d,

        /// <summary>
        /// Error - The specified image file did not have the correct format: it
        /// appears to be LE format.
        /// </summary>
        InvalidImageLeFormat = 0xc000012e,

        /// <summary>
        /// Error - The specified image file did not have the correct format: it
        /// did not have an initial MZ.
        /// </summary>
        InvalidImageNotMz = 0xc000012f,

        /// <summary>
        /// Error - The specified image file did not have the correct format: it
        /// did not have a proper <c>e_lfarlc</c> in the MZ header.
        /// </summary>
        InvalidImageProtect = 0xc0000130,

        /// <summary>
        /// Error - The specified image file did not have the correct format: it
        /// appears to be a 16-bit Windows image.
        /// </summary>
        InvalidImageWin16 = 0xc0000131,

        /// <summary>
        /// Error - The <c>Netlogon</c> service cannot start because another <c>Netlogon</c>
        /// service running in the domain conflicts with the specified role.
        /// </summary>
        LogonServer = 0xc0000132,

        /// <summary>
        /// Error - The time at the primary domain controller is different from
        /// the time at the backup domain controller or member server by too
        /// large an amount.
        /// </summary>
        DifferenceAtDc = 0xc0000133,

        /// <summary>
        /// Error - The SAM database on a Windows Server operating system is
        /// significantly out of synchronization with the copy on the domain
        /// controller. A complete synchronization is required.
        /// </summary>
        SynchronizationRequired = 0xc0000134,

        /// <summary>
        /// Error - {Unable To Locate Component} This application has failed to
        /// start because DLL was not found. Reinstalling the application might
        /// fix this problem.
        /// </summary>
        DllNotFound = 0xc0000135,

        /// <summary>
        /// Error - {Privilege Failed} The I/O permissions for the process could
        /// not be changed.
        /// </summary>
        IoPrivilegeFailed = 0xc0000137,

        /// <summary>
        /// Error - {Ordinal Not Found} The ordinal could not be located in the
        /// dynamic link library.
        /// </summary>
        OrdinalNotFound = 0xc0000138,

        /// <summary>
        /// Error - {Entry Point Not Found} The procedure entry point could
        /// not be located in the dynamic link library.
        /// </summary>
        EntryPointNotFound = 0xc0000139,

        /// <summary>
        /// Error - {Application Exit by CTRL+C} The application terminated as a
        /// result of a CTRL+C.
        /// </summary>
        ControlCExit = 0xc000013a,

        /// <summary>
        /// Error - An attempt to remove a processes DebugPort was made, but a
        /// port was not already associated with the process.
        /// </summary>
        PortNotSet = 0xc0000353,

        /// <summary>
        /// Error - An attempt to do an operation on a debug port failed because
        /// the port is in the process of being deleted.
        /// </summary>
        DebuggerInactive = 0xc0000354,

        /// <summary>
        /// Error - A callback has requested to bypass native code.
        /// </summary>
        CallbackBypass = 0xc0000503,

        /// <summary>
        /// Error - The ALPC port is closed.
        /// </summary>
        PortClosed = 0xc0000700,

        /// <summary>
        /// Error - The ALPC message requested is no longer available.
        /// </summary>
        MessageLost = 0xc0000701,

        /// <summary>
        /// Error - The ALPC message supplied is invalid.
        /// </summary>
        InvalidMessage = 0xc0000702,

        /// <summary>
        /// Error - The ALPC message has been canceled.
        /// </summary>
        RequestCanceled = 0xc0000703,

        /// <summary>
        /// Error - Invalid recursive dispatch attempt.
        /// </summary>
        RecursiveDispatch = 0xc0000704,

        /// <summary>
        /// Error - No receive buffer has been supplied in a synchronous
        /// request.
        /// </summary>
        LpcReceiveBufferExpected = 0xc0000705,

        /// <summary>
        /// Error - The connection port is used in an invalid context.
        /// </summary>
        LpcInvalidConnectionUsage = 0xc0000706,

        /// <summary>
        /// Error - The ALPC port does not accept new request messages.
        /// </summary>
        LpcRequestsNotAllowed = 0xc0000707,

        /// <summary>
        /// Error - The resource requested is already in use.
        /// </summary>
        ResourceInUse = 0xc0000708,

        /// <summary>
        /// Error - Either the target process, or the target thread's containing
        /// process, is a protected process.
        /// </summary>
        ProcessIsProtected = 0xc0000712,

        /// <summary>
        /// Error - The operation could not be completed because the volume is
        /// dirty. Please run the Chkdsk utility and try again.
        /// </summary>
        VolumeDirty = 0xc0000806,

        /// <summary>
        /// Error - This file is checked out or locked for editing by another
        /// user.
        /// </summary>
        FileCheckedOut = 0xc0000901,

        /// <summary>
        /// Error - The file must be checked out before saving changes.
        /// </summary>
        CheckOutRequired = 0xc0000902,

        /// <summary>
        /// Error - The file type being saved or retrieved has been blocked.
        /// </summary>
        BadFileType = 0xc0000903,

        /// <summary>
        /// Error - The file size exceeds the limit allowed and cannot be saved.
        /// </summary>
        FileTooLarge = 0xc0000904,

        /// <summary>
        /// Error - Access Denied. Before opening files in this location, you
        /// must first browse to the e.g. site and select the option to log on
        /// automatically.
        /// </summary>
        FormsAuthRequired = 0xc0000905,

        /// <summary>
        /// Error - The operation did not complete successfully because the file
        /// contains a virus.
        /// </summary>
        VirusInfected = 0xc0000906,

        /// <summary>
        /// Error - This file contains a virus and cannot be opened. Due to the
        /// nature of this virus, the file has been removed from this location.
        /// </summary>
        VirusDeleted = 0xc0000907,

        /// <summary>
        /// Error - Transaction - The function attempted to use a name that is
        /// reserved for use by another transaction.
        /// </summary>
        TransactionalConflict = 0xc0190001,

        /// <summary>
        /// Error - Transaction - The transaction handle associated with this
        /// operation is invalid.
        /// </summary>
        InvalidTransaction = 0xc0190002,

        /// <summary>
        /// Error - Transaction - The requested operation was made in the
        /// context of a transaction that is no longer active.
        /// </summary>
        TransactionNotActive = 0xc0190003,

        /// <summary>
        /// Error - Transaction - The transaction manager was unable to be
        /// successfully initialized. Transacted operations are not supported.
        /// </summary>
        TmInitializationFailed = 0xc0190004,

        /// <summary>
        /// Error - Transaction - Transaction support within the specified file
        /// system resource manager was not started or was shut down due to an
        /// error.
        /// </summary>
        RmNotActive = 0xc0190005,

        /// <summary>
        /// Error - Transaction - The metadata of the resource manager has been
        /// corrupted. The resource manager will not function.
        /// </summary>
        RmMetadataCorrupt = 0xc0190006,

        /// <summary>
        /// Error - Transaction - The resource manager attempted to prepare a
        /// transaction that it has not successfully joined.
        /// </summary>
        TransactionNotJoined = 0xc0190007,

        /// <summary>
        /// Error - Transaction - The specified directory does not contain a
        /// file system resource manager.
        /// </summary>
        DirectoryNotRm = 0xc0190008,

        /// <summary>
        /// Error - Transaction - The log could not be set to the requested
        /// size.
        /// </summary>
        CouldNotResizeLog = 0xc0190009,

        /// <summary>
        /// Error - Transaction - The remote server or share does not support
        /// transacted file operations.
        /// </summary>
        TransactionsUnsupportedRemote = 0xc019000a,

        /// <summary>
        /// Error - Transaction - The requested log size for the file system
        /// resource manager is invalid.
        /// </summary>
        LogResizeInvalidSize = 0xc019000b,

        /// <summary>
        /// Error - Transaction - The remote server sent mismatching version
        /// number or Fid for a file opened with transactions.
        /// </summary>
        RemoteFileVersionMismatch = 0xc019000c,

        /// <summary>
        /// Error - Transaction - The resource manager tried to register a
        /// protocol that already exists.
        /// </summary>
        CrmProtocolAlreadyExists = 0xc019000f,

        /// <summary>
        /// Error - Transaction - The attempt to propagate the transaction
        /// failed.
        /// </summary>
        TransactionPropagationFailed = 0xc0190010,

        /// <summary>
        /// Error - Transaction - The requested propagation protocol was not
        /// registered as a CRM.
        /// </summary>
        CrmProtocolNotFound = 0xc0190011,

        /// <summary>
        /// Error - Transaction - The transaction object already has a superior
        /// enlistment, and the caller attempted an operation that would have
        /// created a new superior. Only a single superior enlistment is
        /// allowed.
        /// </summary>
        TransactionSuperiorExists = 0xc0190012,

        /// <summary>
        /// Error - Transaction - The requested operation is not valid on the
        /// transaction object in its current state.
        /// </summary>
        TransactionRequestNotValid = 0xc0190013,

        /// <summary>
        /// Error - Transaction - The caller has called a response API, but the
        /// response is not expected because the transaction manager did not
        /// issue the corresponding request to the caller.
        /// </summary>
        TransactionNotRequested = 0xc0190014,

        /// <summary>
        /// Error - Transaction - It is too late to perform the requested
        /// operation, because the transaction has already been aborted.
        /// </summary>
        TransactionAlreadyAborted = 0xc0190015,

        /// <summary>
        /// Error - Transaction - It is too late to perform the requested
        /// operation, because the transaction has already been committed.
        /// </summary>
        TransactionAlreadyCommitted = 0xc0190016,

        /// <summary>
        /// Error - Transaction - The buffer passed in to 
        /// <c>NtPushTransaction</c> or
        /// <c>NtPullTransaction</c> is not in a valid format.
        /// </summary>
        TransactionInvalidMarshallBuffer = 0xc0190017,

        /// <summary>
        /// Error - Transaction - The current transaction context associated
        /// with the thread is not a valid handle to a transaction object.
        /// </summary>
        CurrentTransactionNotValid = 0xc0190018,

        /// <summary>
        /// Error - Transaction - An attempt to create space in the
        /// transactional resource manager's log failed. The failure status has
        /// been recorded in the event log.
        /// </summary>
        LogGrowthFailed = 0xc0190019,

        /// <summary>
        /// Error - Transaction - The object (file, stream, or link) that
        /// corresponds to the handle has been deleted by a transaction
        /// save-point rollback.
        /// </summary>
        ObjectNoLongerExists = 0xc0190021,

        /// <summary>
        /// Error - Transaction - The specified file mini-version was not found
        /// for this transacted file open.
        /// </summary>
        StreamMiniversionNotFound = 0xc0190022,

        /// <summary>
        /// Error - Transaction - The specified file mini-version was found but
        /// has been invalidated. The most likely cause is a transaction
        /// save-point rollback.
        /// </summary>
        StreamMiniversionNotValid = 0xc0190023,

        /// <summary>
        /// Error - Transaction - A mini-version can be opened only in the
        /// context of the transaction that created it.
        /// </summary>
        MiniversionInaccessibleFromSpecifiedTransaction = 0xc0190024,

        /// <summary>
        /// Error - Transaction - It is not possible to open a mini-version with
        /// modify access.
        /// </summary>
        CantOpenMiniversionWithModifyIntent = 0xc0190025,

        /// <summary>
        /// Error - Transaction - It is not possible to create any more
        /// mini-versions for this stream.
        /// </summary>
        CantCreateMoreStreamMiniversions = 0xc0190026,

        /// <summary>
        /// Error - Transaction - The handle has been invalidated by a
        /// transaction. The most likely cause is the presence of memory mapping
        /// on a file or an open handle when the transaction ended or rolled
        /// back to save-point.
        /// </summary>
        HandleNoLongerValid = 0xc0190028,

        /// <summary>
        /// Error - Transaction - There is no transaction metadata on the file.
        /// </summary>
        NoTxfMetadata = 0xc0190029,

        /// <summary>
        /// Error - Transaction - The transaction outcome is unavailable because
        /// the resource manager responsible for it is disconnected.
        /// </summary>

        /// <summary>
        /// Error - The transaction outcome is unavailable because the resource
        /// manager responsible for it is disconnected.
        /// </summary>
        RmDisconnected = 0xc0190032,

        /// <summary>
        /// Error - Transaction - The request was rejected because the
        /// enlistment in question is not a superior enlistment.
        /// </summary>
        EnlistmentNotSuperior = 0xc0190033,

        /// <summary>
        /// Error - Transaction - The file cannot be opened in a transaction
        /// because its identity depends on the outcome of an unresolved
        /// transaction.
        /// </summary>

        /// <summary>
        /// Error - The file cannot be opened in a transaction because its
        /// identity depends on the outcome of an unresolved transaction.
        /// </summary>
        FileIdentityNotPersistent = 0xc0190036,

        /// <summary>
        /// Error - Transaction - The operation cannot be performed because
        /// another transaction is depending on this property not changing.
        /// </summary>
        CantBreakTransactionalDependency = 0xc0190037,

        /// <summary>
        /// Error - Transaction - The operation would involve a single file with
        /// two transactional resource managers and is, therefore, not allowed.
        /// </summary>
        CantCrossRmBoundary = 0xc0190038,

        /// <summary>
        /// Error - Transaction - The <c>$Txf</c> directory must be empty for
        /// this operation to succeed.
        /// </summary>
        TxfDirNotEmpty = 0xc0190039,

        /// <summary>
        /// Error - Transaction - The operation would leave a transactional
        /// resource manager in an inconsistent state and is therefore not
        /// allowed.
        /// </summary>
        IndoubtTransactionsExist = 0xc019003a,

        /// <summary>
        /// Error - Transaction - The operation could not be completed because
        /// the transaction manager does not have a log.
        /// </summary>
        TmVolatile = 0xc019003b,

        /// <summary>
        /// Error - Transaction - A rollback could not be scheduled because a
        /// previously scheduled rollback has already executed or been queued
        /// for execution.
        /// </summary>
        RollbackTimerExpired = 0xc019003c,

        /// <summary>
        /// Error - Transaction - The transactional metadata attribute on the
        /// file or directory is corrupt and unreadable.
        /// </summary>
        TxfAttributeCorrupt = 0xc019003d,

        /// <summary>
        /// Error - Transaction - The encryption operation could not be
        /// completed because a transaction is active.
        /// </summary>
        EfsNotAllowedInTransaction = 0xc019003e,

        /// <summary>
        /// Error - Transaction - This object is not allowed to be opened in a
        /// transaction.
        /// </summary>
        TransactionalOpenNotAllowed = 0xc019003f,

        /// <summary>
        /// Error - Transaction - Memory mapping (creating a mapped section) a
        /// remote file under a transaction is not supported.
        /// </summary>
        TransactedMappingUnsupportedRemote = 0xc0190040,

        /// <summary>
        /// Error - Transaction - Promotion was required to allow the resource
        /// manager to enlist, but the transaction was set to disallow it.
        /// </summary>

        /// <summary>
        /// Error - Promotion was required to allow the resource manager to
        /// enlist, but the transaction was set to disallow it.
        /// </summary>
        TransactionRequiredPromotion = 0xc0190043,

        /// <summary>
        /// Error - Transaction - This file is open for modification in an
        /// unresolved transaction and can be opened for execute only by a
        /// transacted reader.
        /// </summary>
        CannotExecuteFileInTransaction = 0xc0190044,

        /// <summary>
        /// Error - Transaction - The request to thaw frozen transactions was
        /// ignored because transactions were not previously frozen.
        /// </summary>
        TransactionsNotFrozen = 0xc0190045,

        /// <summary>
        /// Maximal value
        /// </summary>
        MaximumNtStatus = 0xffffffff
    }
}
﻿// ------------------------------------------------------------------------------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------------------------------------------------------------------------------

using System.Threading.Tasks;

namespace FileService.Interfaces
{
    /// <summary>
    /// Provides a contract for reading from and writing to file sources.
    /// </summary>
    public interface IFileUtility
    {
        /// <summary>
        /// Reads file contents from a blob storage account given the source of the path
        /// </summary>
        /// <param name="filePathSource"></param>
        Task<string> ReadFromFile(string filePathSource);

        /// <summary>
        /// Allows one to make changes to a file
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="filePathSource"></param>
        Task WriteToFile(string fileContents, string filePathSource);
    }
}

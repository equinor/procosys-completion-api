# 4. New blob storage
Date 2024-01-24

## Status

Accepted

## Context

In PCS 4 we store attachments as blobs in an Azure storage container with the following file structure: 
\<container\>/\<plant\>/\<Trunc(\<file_id\>/1000)*1000/\<file_id\>.jpg. 
The structure is based on how things where stored earlier, when the files where stored in folders, with 1000 files in each folder (based on a limit in the operation system).  
To add a new blob, we use Oracle SEQUENCE to find the next id. This id is used to calculate what folder to use and the name on the file. 
There have been incidents in the system causing two attachments pointing to the same blob. This is a known bug with Sequence in Oracle.

For IPO and Preservation we have used a simpler structure, where we use Guid instead of id, and without needing to calculate the folder based on an id.  
There we have the following structure:  \<container\>/\<plant\>/\<objecttype\>/\<guid\>/\<name on file\>.

By having the same solution for PCS 4 and PCS 5, it will simply the overall solution. 

## Decision

For Completion we will have a new storage container for attachment blobs, with the same file-structure as for IPO and Preservation. 
All blobs (for all entitites) in the old storage container will be moved to the new storage container with the new structure.   

## Consequences

We need to copy all blobs from PCS 4 storage container to the new storage container for Completion, prior to going live with the first version of Completion. 
To do this task we can use Azure Storage Data Movement Library for .Net.

We will not synchronize changes from new to old storage container. We will instead modify PCS 4 to use the new storage container (must be set in production before first version of Completion goes live). 

Before deploying changes on use of storage container to PCS 4, we will synchronize all blobs. The system need to be taken down, to ensure that we can copy over any last minute blob changes. 

	
# 4. New blob storage
Date 2024-01-24

## Status

Accepted

## Context

In PCS 4 we store attachments as blobs in an Azure storage container with the following file structure: \<container\>/\<plant\>/\<Trunc(\<file_id\>/1000)*1000/\<file_id\>.jpg. The structure is based on how things where stored earlier, when the files where stored in folders, with 1000 files in each folder (based on a limit for folders).  To add a new blob, we use Oracle SEQUENCE to find the next id. This id is used to calculate what folder to use and the name on the file. There have been incidents in the system causing two attachments pointing to the same blob.

For IPO and Preservation we have used a simpler structure, where we use Guid instead of id, and without needing to calculate the folder based on an id.  We have the following structure:  \<container\>/\<plant\>/\<objekttype\>/\<guid\>/bilde.jpg.

## Decision

For Completion we will have a new storage container for attachment blobs, with the same file-structure as for IPO and Preservation. All blobs (for all entitites) in the old storage container will be moved to the new. 

## Consequences

We need to copy all blobs from PCS 4 storage container to the new storage container for Completion, prior to going live with the first version of Completion. 
To do this task we can use Azure Storage Data Movement Library for .Net".

During development of PCS 5, the attachments need to be available both for PCS 4 and PCS 5. 
We will not synchronize changes from new to old storage container. We will instead modify PCS 4 to use the new storage container (must be set in production when Completion goes live). 


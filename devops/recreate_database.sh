#!/bin/bash -e

if [ -z "$RavenDB_URL" ]; then
    echo "RavenDB_URL env var is required."
    exit 1
fi

if [ -z "$RavenDB_DB_Name" ]; then
    echo "RavenDB_DB_Name env var is required."
    exit 2
fi

createArgs="-X PUT $RavenDB_URL/admin/databases?name=$RavenDB_DB_Name&replicationFactor=1 --data-raw {'DatabaseName':'$RavenDB_DB_Name','Settings':{},'Disabled':false,'Encrypted':false,'Topology':{'DynamicNodesDistribution':false}}"

deleteArgs="-X DELETE $RavenDB_URL/admin/databases --data-raw {'HardDelete':true,'DatabaseNames':['$RavenDB_DB_Name']}"

importArgs="$RavenDB_URL/databases/$RavenDB_DB_Name/smuggler/import -F importOptions={'IncludeExpired':false,'IncludeArtificial':false,'RemoveAnalyzers':false,'OperateOnTypes':'Documents,Indexes,Identities','OperateOnDatabaseRecordTypes':'None'} -F file=@documentation/exported_data.ravendbdump"

certificateArgs="-X POST $RavenDB_URL/admin/certificates/edit --data-raw {'Name':'app','Thumbprint':'B219C9650F17AE7252007551263C60B3104A9485','SecurityClearance':'ValidUser','Permissions':{'$RavenDB_DB_Name':'ReadWrite'}}"

for arg in "$deleteArgs" "$createArgs" "$importArgs" "$certificateArgs"
do
    echo $arg
    curl --silent --show-error --fail $arg --key ./crt.key --cert ./crt.pem || exit 3
    echo ""
done

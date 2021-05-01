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

for arg in "$deleteArgs" "$createArgs" "$importArgs"
do
    echo $arg
	curl --silent --show-error --fail $arg --key ./crt.key --cert ./crt.pem || exit 3
    echo ""
done

param($TargetRepo = "ravendbsamples/yabt")

docker tag yabt $TargetRepo
docker push $TargetRepo
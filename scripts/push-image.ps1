param($TargetRepo)

docker tag yabt $TargetRepo
docker push $TargetRepo
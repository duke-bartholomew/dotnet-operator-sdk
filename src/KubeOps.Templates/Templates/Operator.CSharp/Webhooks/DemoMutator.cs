﻿using GeneratedOperatorProject.Entities;
using KubeOps.Operator.Webhooks;
using Microsoft.AspNetCore.Http;

namespace GeneratedOperatorProject.Webhooks
{
    public class DemoMutator : IMutationWebhook<V1DemoEntity>
    {
        public AdmissionOperations Operations => AdmissionOperations.Create;

        public MutationResult Create(V1DemoEntity newEntity, bool dryRun)
        {
            newEntity.Spec.Username = "not foobar";
            return MutationResult.Modified(newEntity);
        }
    }
}

# Playbook runbook

Stands in for: the Logic App playbook you build in the portal. Not a workflow export.

1. Create a Consumption logic app. Standard is allowed only if it is stateful. Sentinel does not support Standard stateless workflows.
2. Trigger: Microsoft Sentinel incident. Do not rename that trigger after creation.
3. Action: Azure Functions, targeting CommentIncident.
4. Action: Add comment to incident, using the incident ARM id and the function body.
5. Grant Microsoft Sentinel Automation Contributor on the playbook resource group.

In the Defender portal, create the automation rule from Microsoft Sentinel, Configuration, Automation, not from an incident. After 31 March 2027 the Azure portal is not supported for Sentinel.

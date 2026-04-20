<#ftl output_format="plainText">
<#assign accountLabel = (user.email)!((user.username)!"")>
<#assign safeExpiration = linkExpiration!0>
<#assign requiredActionsText>
<#if requiredActions?? && requiredActions?has_content>
<#list requiredActions as reqActionItem>${msg("requiredAction.${reqActionItem}")}<#sep>, </#sep></#list>
<#else>
${msg("ofExecuteActionsFallbackActions")}
</#if>
</#assign>
${msg("ofExecuteActionsTitle")}

${msg("ofExecuteActionsLead", (realmName)!"", accountLabel, requiredActionsText)}

<#if requiredActions?? && requiredActions?has_content>
<#list requiredActions as reqActionItem>
- ${msg("requiredAction.${reqActionItem}")}
</#list>
</#if>

${msg("ofExecuteActionsButtonLabel")}:
<#if link??>${link}<#else>${msg("ofExecuteActionsMissingLink")}</#if>

${msg("ofExecuteActionsExpiry", linkExpirationFormatter(safeExpiration))}

${msg("ofExecuteActionsSecurity")}

--
${msg("ofExecuteActionsSignature")}

${msg("ofExecuteActionsFooter", (realmName)!"")}

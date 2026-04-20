<#ftl output_format="plainText">
<#assign accountLabel = (user.email)!((user.username)!"")>
${msg("ofPasswordResetTitle")}

${msg("ofPasswordResetLead", realmName, accountLabel)}

${msg("ofPasswordResetButtonLabel")}:
${link}

${msg("ofPasswordResetExpiry", linkExpirationFormatter(linkExpiration))}

${msg("ofPasswordResetSecurity")}

--
${msg("ofPasswordResetSignature")}

${msg("ofPasswordResetFooter", realmName)}

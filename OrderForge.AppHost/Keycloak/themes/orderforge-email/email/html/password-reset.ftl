<#import "template.ftl" as layout>
<#assign accountLabel = (user.email)!((user.username)!"")>
<@layout.emailLayout>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html lang="en" xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office">
<head>
  <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <meta name="x-apple-disable-message-reformatting" />
  <meta name="format-detection" content="telephone=no,address=no,email=no,date=no,url=no" />
  <title>${msg("passwordResetSubject")}</title>
  <!--[if mso]>
  <noscript>
    <xml>
      <o:OfficeDocumentSettings>
        <o:PixelsPerInch>96</o:PixelsPerInch>
      </o:OfficeDocumentSettings>
    </xml>
  </noscript>
  <![endif]-->
</head>
<body style="margin:0;padding:0;background-color:#f3f4f6;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;">
  <div style="display:none;font-size:1px;color:#f3f4f6;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;mso-hide:all;">
    ${msg("ofPasswordResetPreheader")}
  </div>
  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f3f4f6;border-collapse:collapse;mso-table-lspace:0;mso-table-rspace:0;">
    <tr>
      <td align="center" style="padding:24px 16px;">
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:600px;border-collapse:collapse;mso-table-lspace:0;mso-table-rspace:0;">
          <tr>
            <td style="padding:0 0 20px 0;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:13px;line-height:18px;color:#6b7280;text-align:center;">
              <span style="font-size:20px;font-weight:700;letter-spacing:-0.02em;color:#111827;">Order Forge</span>
            </td>
          </tr>
          <tr>
            <td style="background-color:#ffffff;border-radius:8px;border:1px solid #e5e7eb;box-shadow:0 1px 2px rgba(0,0,0,0.05);">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;mso-table-lspace:0;mso-table-rspace:0;">
                <tr>
                  <td style="padding:32px 28px 8px 28px;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:22px;line-height:28px;font-weight:600;color:#111827;">
                    ${msg("ofPasswordResetTitle")}
                  </td>
                </tr>
                <tr>
                  <td style="padding:16px 28px 0 28px;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:15px;line-height:24px;color:#374151;">
                    ${kcSanitize(msg("ofPasswordResetLead", realmName, accountLabel))?no_esc}
                  </td>
                </tr>
                <tr>
                  <td align="center" style="padding:28px 28px 8px 28px;">
                    <!--[if mso]>
                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="border-collapse:collapse;"><tr><td align="center" bgcolor="#0066cc" style="border-radius:6px;">
                    <![endif]-->
                    <a href="${link}" target="_blank" rel="noopener noreferrer" style="display:inline-block;padding:14px 28px;background-color:#0066cc;color:#ffffff !important;font-weight:600;text-decoration:none;border-radius:6px;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:16px;line-height:20px;mso-line-height-rule:exactly;">
                      ${msg("ofPasswordResetButtonLabel")}
                    </a>
                    <!--[if mso]>
                    </td></tr></table>
                    <![endif]-->
                  </td>
                </tr>
                <tr>
                  <td style="padding:0 28px 24px 28px;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:13px;line-height:20px;color:#6b7280;text-align:center;">
                    ${msg("ofPasswordResetExpiry", linkExpirationFormatter(linkExpiration))}
                  </td>
                </tr>
                <tr>
                  <td style="padding:0 28px 24px 28px;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:12px;line-height:18px;color:#6b7280;">
                    ${kcSanitize(msg("ofPasswordResetUrlAlternative"))?no_esc}<br />
                    <span style="font-family:Consolas,Menlo,Monaco,monospace;font-size:11px;line-height:16px;color:#4b5563;word-break:break-all;">${link?html}</span>
                  </td>
                </tr>
                <tr>
                  <td style="padding:0 28px 16px 28px;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:14px;line-height:22px;color:#374151;border-top:1px solid #f3f4f6;">
                    ${kcSanitize(msg("ofPasswordResetSecurity"))?no_esc}
                  </td>
                </tr>
                <tr>
                  <td style="padding:0 28px 32px 28px;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:14px;line-height:22px;color:#374151;">
                    ${kcSanitize(msg("ofPasswordResetSignatureHtml"))?no_esc}
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style="padding:24px 8px 0 8px;font-family:Segoe UI,Roboto,Helvetica Neue,Arial,sans-serif;font-size:12px;line-height:18px;color:#9ca3af;text-align:center;">
              ${kcSanitize(msg("ofPasswordResetFooter", realmName))?no_esc}
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
</@layout.emailLayout>

<#import "template.ftl" as layout>
<@layout.registrationLayout displayInfo=false displayMessage=!messagesPerField.existsError('username'); section>

  <#if section = "header">
    ${msg("emailForgotTitle")}
  <#elseif section = "form">
    <div class="forge-login-page">
      <div class="forge-login-left">
        <div class="forge-login-card">

          <div class="forge-brand">
            <img class="forge-brand-image" src="${url.resourcesPath}/img/logo.svg" alt="Top Of The Range" />
            <div class="forge-brand-name">Top Of The Range</div>
          </div>

          <h1 class="forge-title">${msg("emailForgotTitle")}</h1>
          <p class="forge-subtitle"><#if realm.duplicateEmailsAllowed>${msg("emailInstructionUsername")}<#else>${msg("emailInstruction")}</#if></p>

          <#if message?has_content>
            <div class="pf-v5-c-alert pf-m-inline pf-m-${message.type}">
              <div class="pf-v5-c-alert__title">${kcSanitize(message.summary)?no_esc}</div>
            </div>
          </#if>

          <form id="kc-reset-password-form" class="forge-form" action="${url.loginAction}" method="post">
            <#assign usernameLabel>
              <#if !realm.loginWithEmailAllowed>${msg("username")}<#elseif !realm.registrationEmailAsUsername>${msg("usernameOrEmail")}<#else>${msg("email")}</#if>
            </#assign>

            <div class="forge-field">
              <label for="username">${usernameLabel}</label>
              <input
                tabindex="1"
                id="username"
                class="forge-input"
                name="username"
                value="${auth.attemptedUsername!''}"
                type="text"
                autofocus
                autocomplete="username"
                aria-invalid="<#if messagesPerField.existsError('username')>true</#if>"
              />
              <#if messagesPerField.existsError('username')>
                <div class="pf-v5-c-form__helper-text pf-m-error">
                  ${kcSanitize(messagesPerField.getFirstError('username'))?no_esc}
                </div>
              </#if>
            </div>

            <div class="forge-actions forge-actions--stack">
              <input
                class="forge-submit"
                id="kc-form-buttons"
                name="login"
                type="submit"
                value="${msg('doSubmit')}"
                tabindex="2"
              />
              <a class="forge-link forge-link--centered" href="${url.loginUrl}" tabindex="3">${msg("backToLogin")}</a>
            </div>
          </form>

        </div>
      </div>

      <div class="forge-login-right">
        <div class="forge-login-overlay">
          <h2 class="forge-hero-title">Streamline Your B2B Ordering</h2>
          <p class="forge-hero-text">
            Access your customer-specific pricing, manage orders, track shipments,
            and reorder with ease.
          </p>

          <ul class="forge-feature-list">
            <li>Custom pricing for your business</li>
            <li>Real-time inventory and stock levels</li>
            <li>Fast reordering from order history</li>
            <li>Net 30 payment terms on account</li>
          </ul>
        </div>
      </div>
    </div>

  </#if>
</@layout.registrationLayout>

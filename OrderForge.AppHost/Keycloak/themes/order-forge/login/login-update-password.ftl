<#import "template.ftl" as layout>
<#import "password-commons.ftl" as passwordCommons>
<#import "password-validation.ftl" as validator>
<@layout.registrationLayout displayMessage=!messagesPerField.existsError('password','password-confirm'); section>

  <#if section = "header">
    ${msg("updatePasswordTitle")}
  <#elseif section = "form">

    <div class="forge-login-page">
      <div class="forge-login-left">
        <div class="forge-login-card">

          <div class="forge-brand">
            <img class="forge-brand-image" src="${url.resourcesPath}/img/logo.svg" alt="Top Of The Range" />
            <div class="forge-brand-name">Top Of The Range</div>
          </div>

          <h1 class="forge-title">${msg("updatePasswordTitle")}</h1>

          <#if message?has_content>
            <div class="pf-v5-c-alert pf-m-inline pf-m-${message.type}">
              <div class="pf-v5-c-alert__title">${kcSanitize(message.summary)?no_esc}</div>
            </div>
          </#if>

          <form id="kc-passwd-update-form" class="forge-form" onsubmit="login.disabled = true; return true;" action="${url.loginAction}" method="post" novalidate="novalidate">

            <div class="forge-field">
              <label for="password-new">${msg("passwordNew")}</label>
              <input
                type="password"
                id="password-new"
                name="password-new"
                class="forge-input"
                autocomplete="new-password"
                autofocus
                aria-invalid="<#if messagesPerField.existsError('password','password-new')>true</#if>"
              />
              <#if messagesPerField.existsError('password-new')>
                <div class="pf-v5-c-form__helper-text pf-m-error">
                  ${kcSanitize(messagesPerField.getFirstError('password-new'))?no_esc}
                </div>
              <#elseif messagesPerField.existsError('password')>
                <div class="pf-v5-c-form__helper-text pf-m-error">
                  ${kcSanitize(messagesPerField.getFirstError('password'))?no_esc}
                </div>
              </#if>
            </div>

            <div class="forge-field">
              <label for="password-confirm">${msg("passwordConfirm")}</label>
              <input
                type="password"
                id="password-confirm"
                name="password-confirm"
                class="forge-input"
                autocomplete="new-password"
                aria-invalid="<#if messagesPerField.existsError('password-confirm')>true</#if>"
              />
              <#if messagesPerField.existsError('password-confirm')>
                <div class="pf-v5-c-form__helper-text pf-m-error">
                  ${kcSanitize(messagesPerField.getFirstError('password-confirm'))?no_esc}
                </div>
              </#if>
            </div>

            <div class="forge-password-extras">
              <@passwordCommons.logoutOtherSessions/>
            </div>

            <div class="forge-actions<#if isAppInitiatedAction??> forge-actions--split</#if>">
              <#if isAppInitiatedAction??>
                <input class="forge-submit" name="login" id="kc-submit" type="submit" value="${msg('doSubmit')}" />
                <button type="submit" name="cancel-aia" id="kc-cancel" class="forge-button-secondary">${msg("doCancel")}</button>
              <#else>
                <input class="forge-submit" name="login" id="kc-submit" type="submit" value="${msg('doSubmit')}" />
              </#if>
            </div>
          </form>

          <@validator.templates/>
          <@validator.script field="password-new"/>

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

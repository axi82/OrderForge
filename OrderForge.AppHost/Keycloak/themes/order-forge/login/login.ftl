<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=!messagesPerField.existsError('username','password') displayInfo=realm.password && realm.registrationAllowed && !registrationDisabled??; section>

  <#if section = "header">
    Welcome Back
  <#elseif section = "form">
    <div class="forge-login-page">
      <div class="forge-login-left">
        <div class="forge-login-card">

		<div class="forge-brand">
		  <img class="forge-brand-image" src="${url.resourcesPath}/img/logo.svg" alt="Top Of The Range" />
		  <div class="forge-brand-name">Top Of The Range</div>
		</div>

          <h1 class="forge-title">Welcome Back</h1>
          <p class="forge-subtitle">Self-Service Ordering for Your Business</p>

          <#if message?has_content>
            <div class="pf-v5-c-alert pf-m-inline pf-m-${message.type}">
              <div class="pf-v5-c-alert__title">${kcSanitize(message.summary)?no_esc}</div>
            </div>
          </#if>

          <form id="kc-form-login" class="forge-form" onsubmit="login.disabled = true; return true;" action="${url.loginAction}" method="post">

            <div class="forge-field">
              <label for="username">
                <#if !realm.loginWithEmailAllowed>Email Address
                <#elseif !realm.registrationEmailAsUsername>Email Address
                <#else>Email Address
                </#if>
              </label>

              <input
                tabindex="1"
                id="username"
                class="forge-input"
                name="username"
                value="${(login.username!'')}"
                type="text"
                autofocus
                autocomplete="username"
                placeholder="your.email@company.com"
                aria-invalid="<#if messagesPerField.existsError('username','password')>true</#if>"
              />

              <#if messagesPerField.existsError('username','password')>
                <div class="pf-v5-c-form__helper-text pf-m-error">
                  ${kcSanitize(messagesPerField.getFirstError('username','password'))?no_esc}
                </div>
              </#if>
            </div>

            <div class="forge-row">
              <label for="password">Password</label>
              <#if realm.resetPasswordAllowed>
                <a class="forge-link" href="${url.loginResetCredentialsUrl}">Forgot password?</a>
              </#if>
            </div>

            <div class="forge-field">
              <input
                tabindex="2"
                id="password"
                class="forge-input"
                name="password"
                type="password"
                autocomplete="current-password"
                placeholder="Enter your password"
              />
            </div>

            <#if realm.rememberMe && !usernameEditDisabled??>
              <div class="forge-remember">
                <label>
                  <input id="rememberMe" name="rememberMe" type="checkbox" tabindex="3"
                    <#if login.rememberMe??>checked</#if>> Remember me
                </label>
              </div>
            </#if>

            <div class="forge-actions">
              <input
                class="forge-submit"
                name="login"
                id="kc-login"
                type="submit"
                value="Sign In"
                tabindex="4"
              />
            </div>
          </form>

          <#if realm.password && realm.registrationAllowed && !registrationDisabled??>
            <div class="forge-register">
              Don’t have an account?
              <a class="forge-link" href="${url.registrationUrl}">Register your company</a>
            </div>
          </#if>

          <div class="forge-demo-box">
            <strong>Demo Mode:</strong> Enter any credentials to continue
          </div>

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

  <#elseif section = "info">
  </#if>
</@layout.registrationLayout>
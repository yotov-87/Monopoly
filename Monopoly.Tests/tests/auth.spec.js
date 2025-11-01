const { test, expect } = require('@playwright/test');

/**
 * Generates a unique username and password based on current timestamp
 * Pattern: AutoTestUser<<ddMMyyHHmmss>>
 * Example: AutoTestUser011125153033 (01-Nov-2025 15:30:33)
 */
function generateTestCredentials() {
  const now = new Date();
  const day = String(now.getDate()).padStart(2, '0');
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const year = String(now.getFullYear()).slice(-2);
  const hours = String(now.getHours()).padStart(2, '0');
  const minutes = String(now.getMinutes()).padStart(2, '0');
  const seconds = String(now.getSeconds()).padStart(2, '0');
  
  const username = `AutoTestUser${day}${month}${year}${hours}${minutes}${seconds}`;
  return {
    username,
    password: username // Password is the same as username
  };
}

test.describe('Authentication Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Clear local storage before each test
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());
  });

  test('should register a new user successfully', async ({ page }) => {
    const { username, password } = generateTestCredentials();
    
    // Navigate to registration page
    await page.goto('/register');
    
    // Fill registration form
    await page.locator('#register-username').fill(username);
    await page.locator('#register-password').fill(password);
    await page.locator('#register-confirm-password').fill(password);
    
    // Submit form
    await page.locator('#register-submit-btn').click();
    
    // Wait for redirect to home page
    await page.waitForURL('/');
    
    // Verify user is logged in (Logout button should be visible)
    await expect(page.locator('#header-logout-btn')).toBeVisible();
    
    // Verify Login and Register buttons are not visible
    await expect(page.locator('#header-login-btn')).not.toBeVisible();
    await expect(page.locator('#header-register-btn')).not.toBeVisible();
    
    // Verify token is stored in localStorage
    const token = await page.evaluate(() => localStorage.getItem('authToken'));
    expect(token).toBeTruthy();
    expect(token).not.toBe('');
  });

  test('should login with existing user', async ({ page }) => {
    const { username, password } = generateTestCredentials();
    
    // First register a user
    await page.goto('/register');
    await page.locator('#register-username').fill(username);
    await page.locator('#register-password').fill(password);
    await page.locator('#register-confirm-password').fill(password);
    await page.locator('#register-submit-btn').click();
    await page.waitForURL('/');
    
    // Logout
    await page.locator('#header-logout-btn').click();
    
    // Verify logged out state
    await expect(page.locator('#header-login-btn')).toBeVisible();
    await expect(page.locator('#header-register-btn')).toBeVisible();
    
    // Now login with the same credentials
    await page.locator('#header-login-btn').click();
    await page.waitForURL('/login');
    
    await page.locator('#login-username').fill(username);
    await page.locator('#login-password').fill(password);
    await page.locator('#login-submit-btn').click();
    
    // Wait for redirect to home page
    await page.waitForURL('/');
    
    // Verify user is logged in
    await expect(page.locator('#header-logout-btn')).toBeVisible();
    
    // Verify token is stored in localStorage
    const token = await page.evaluate(() => localStorage.getItem('authToken'));
    expect(token).toBeTruthy();
  });

  test('should show error when passwords do not match during registration', async ({ page }) => {
    const { username } = generateTestCredentials();
    
    await page.goto('/register');
    
    await page.locator('#register-username').fill(username);
    await page.locator('#register-password').fill('password1');
    await page.locator('#register-confirm-password').fill('password2');
    
    // Listen for alert dialog
    let dialogMessage = '';
    page.on('dialog', async dialog => {
      dialogMessage = dialog.message();
      await dialog.accept();
    });
    
    await page.locator('#register-submit-btn').click();
    
    // Wait a bit for dialog to appear
    await page.waitForTimeout(500);
    
    // Verify error message
    expect(dialogMessage).toContain('Passwords do not match');
    
    // Verify we're still on register page
    await expect(page).toHaveURL('/register');
  });

  test('should logout and clear token', async ({ page }) => {
    const { username, password } = generateTestCredentials();
    
    // Register and login
    await page.goto('/register');
    await page.locator('#register-username').fill(username);
    await page.locator('#register-password').fill(password);
    await page.locator('#register-confirm-password').fill(password);
    await page.locator('#register-submit-btn').click();
    await page.waitForURL('/');
    
    // Verify logged in
    await expect(page.locator('#header-logout-btn')).toBeVisible();
    
    // Logout
    await page.locator('#header-logout-btn').click();
    
    // Verify Login and Register buttons are visible again
    await expect(page.locator('#header-login-btn')).toBeVisible();
    await expect(page.locator('#header-register-btn')).toBeVisible();
    
    // Verify token is removed from localStorage
    const token = await page.evaluate(() => localStorage.getItem('authToken'));
    expect(token).toBeNull();
  });

  test('should redirect authenticated users away from login page', async ({ page }) => {
    const { username, password } = generateTestCredentials();
    
    // Register and login
    await page.goto('/register');
    await page.locator('#register-username').fill(username);
    await page.locator('#register-password').fill(password);
    await page.locator('#register-confirm-password').fill(password);
    await page.locator('#register-submit-btn').click();
    await page.waitForURL('/');
    
    // Verify logged in
    await expect(page.locator('#header-logout-btn')).toBeVisible();
    
    // Try to navigate to login page
    await page.goto('/login');
    
    // Should be redirected to home
    await expect(page).toHaveURL('/');
    await expect(page.locator('#header-logout-btn')).toBeVisible();
  });

  test('should redirect authenticated users away from register page', async ({ page }) => {
    const { username, password } = generateTestCredentials();
    
    // Register and login
    await page.goto('/register');
    await page.locator('#register-username').fill(username);
    await page.locator('#register-password').fill(password);
    await page.locator('#register-confirm-password').fill(password);
    await page.locator('#register-submit-btn').click();
    await page.waitForURL('/');
    
    // Verify logged in
    await expect(page.locator('#header-logout-btn')).toBeVisible();
    
    // Try to navigate to register page again
    await page.goto('/register');
    
    // Should be redirected to home
    await expect(page).toHaveURL('/');
    await expect(page.locator('#header-logout-btn')).toBeVisible();
  });

  test('should display login and register buttons when not authenticated', async ({ page }) => {
    await page.goto('/');
    
    // Verify Login and Register buttons are visible
    await expect(page.locator('#header-login-btn')).toBeVisible();
    await expect(page.locator('#header-register-btn')).toBeVisible();
    
    // Verify Logout button is not visible
    await expect(page.locator('#header-logout-btn')).not.toBeVisible();
  });

  test('should navigate to registration page when clicking Register button', async ({ page }) => {
    await page.goto('/');
    
    await page.locator('#header-register-btn').click();
    
    // Verify URL
    await expect(page).toHaveURL('/register');
    
    // Verify form elements are present
    await expect(page.locator('#register-username')).toBeVisible();
    await expect(page.locator('#register-password')).toBeVisible();
    await expect(page.locator('#register-confirm-password')).toBeVisible();
    await expect(page.locator('#register-submit-btn')).toBeVisible();
  });

  test('should navigate to login page when clicking Login button', async ({ page }) => {
    await page.goto('/');
    
    await page.locator('#header-login-btn').click();
    
    // Verify URL
    await expect(page).toHaveURL('/login');
    
    // Verify form elements are present
    await expect(page.locator('#login-username')).toBeVisible();
    await expect(page.locator('#login-password')).toBeVisible();
    await expect(page.locator('#login-submit-btn')).toBeVisible();
  });
});

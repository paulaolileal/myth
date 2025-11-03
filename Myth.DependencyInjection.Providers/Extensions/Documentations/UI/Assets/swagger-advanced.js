/**
 * Myth Advanced Swagger UI JavaScript
 * Provides enhanced functionality for Swagger UI
 */

class MythSwaggerUI {
    constructor(config = {}) {
        this.config = {
            // TreeView settings
            treeView: {
                enableHierarchy: true,
                tagSeparator: '/',
                enableCollapse: true,
                expandByDefault: false,
                showEndpointCount: true,
                ...config.treeView
            },

            // Search settings
            search: {
                enableRealTime: true,
                minSearchLength: 2,
                debounceMs: 300,
                enableHighlighting: true,
                caseSensitive: false,
                ...config.search
            },

            // Theme settings
            theme: {
                defaultTheme: 'auto',
                allowUserToggle: true,
                persistPreference: true,
                enableTransitions: true,
                ...config.theme
            },

            // Cache settings
            cache: {
                enablePersistence: true,
                expirationMinutes: 60,
                enableHistory: true,
                keyPrefix: 'swagger_cache_',
                ...config.cache
            },

            // UI settings
            ui: {
                enableKeyboardShortcuts: true,
                enableDirectExecution: true,
                enableJsonBeautify: true,
                enableModelCollapse: true,
                ...config.ui
            },

            // Performance settings
            performance: {
                enableTiming: true,
                enableStatusColors: true,
                enableProgressIndicators: true,
                showToasts: false,
                ...config.performance
            }
        };

        this.cache = new SwaggerCache(this.config.cache);
        this.search = new SwaggerSearch(this.config.search);
        this.theme = new SwaggerTheme(this.config.theme);
        this.shortcuts = new SwaggerKeyboardShortcuts(this.config.ui);
        this.performance = new SwaggerPerformance(this.config.performance);

        this.endpoints = [];
        this.isInitialized = false;
    }

    async initialize() {
        if (this.isInitialized) return;

        try {
            await this.waitForSwaggerUI();
            await this.setupUI();
            await this.loadEndpoints();

            this.isInitialized = true;
            console.log('🚀 Myth Advanced Swagger UI initialized successfully');
        } catch (error) {
            console.error('❌ Failed to initialize Myth Advanced Swagger UI:', error);
        }
    }

    async waitForSwaggerUI() {
        let attempts = 0;
        const maxAttempts = 50;

        while (attempts < maxAttempts) {
            if (window.ui && document.querySelector('.swagger-ui')) {
                await new Promise(resolve => setTimeout(resolve, 500)); // Wait for full render
                return;
            }
            await new Promise(resolve => setTimeout(resolve, 100));
            attempts++;
        }

        throw new Error('Swagger UI not found after maximum attempts');
    }

    async setupUI() {
        this.injectAdvancedHTML();
        this.theme.initialize();
        this.search.initialize();
        this.shortcuts.initialize();
        this.performance.initialize();

        if (this.config.treeView.enableHierarchy) {
            this.setupTreeView();
        }

        if (this.config.ui.enableDirectExecution) {
            this.setupDirectExecution();
        }

        if (this.config.ui.enableModelCollapse) {
            this.setupModelCollapse();
        }
    }

    injectAdvancedHTML() {
        const container = document.querySelector('.swagger-ui');
        if (!container) return;

        // Create advanced search bar
        const searchHTML = `
            <div class="swagger-advanced-search">
                <div class="swagger-search-container">
                    <div style="position: relative; flex: 1;">
                        <input
                            type="text"
                            placeholder="🔍 Search endpoints by name, method, description..."
                            class="swagger-search-input"
                            id="swagger-search-input"
                        />
                        <div class="swagger-search-results" id="swagger-search-results"></div>
                    </div>
                    ${this.config.theme.allowUserToggle ? `
                        <button class="swagger-theme-toggle" id="swagger-theme-toggle" title="Toggle theme">
                            🌓
                        </button>
                    ` : ''}
                </div>
            </div>
        `;

        container.insertAdjacentHTML('afterbegin', searchHTML);
    }

    async loadEndpoints() {
        try {
            const spec = window.ui.getSpec();
            if (!spec || !spec.paths) return;

            this.endpoints = [];

            Object.entries(spec.paths).forEach(([path, methods]) => {
                Object.entries(methods).forEach(([method, operation]) => {
                    if (typeof operation !== 'object' || !operation) return;

                    this.endpoints.push({
                        path,
                        method: method.toUpperCase(),
                        operationId: operation.operationId,
                        summary: operation.summary || '',
                        description: operation.description || '',
                        tags: operation.tags || [],
                        parameters: operation.parameters || [],
                        element: null // Will be populated when DOM is ready
                    });
                });
            });

            // Wait for DOM to be populated with endpoints
            setTimeout(() => this.linkEndpointsToDOM(), 1000);
        } catch (error) {
            console.error('Failed to load endpoints:', error);
        }
    }

    linkEndpointsToDOM() {
        this.endpoints.forEach(endpoint => {
            const selector = `[data-path="${endpoint.path}"][data-method="${endpoint.method.toLowerCase()}"]`;
            endpoint.element = document.querySelector(selector) ||
                document.querySelector(`#operations-${endpoint.tags[0]}-${endpoint.operationId}`);
        });
    }

    setupTreeView() {
        if (!this.config.treeView.enableHierarchy) return;

        const tagGroups = this.groupByTags();
        const treeViewHTML = this.generateTreeViewHTML(tagGroups);

        const container = document.querySelector('.swagger-ui .information-container');
        if (container) {
            container.insertAdjacentHTML('afterend', `
                <div class="swagger-tree-view">
                    <h3>📋 API Endpoints</h3>
                    ${treeViewHTML}
                </div>
            `);

            this.bindTreeViewEvents();
        }
    }

    groupByTags() {
        const groups = {};
        const separator = this.config.treeView.tagSeparator;

        this.endpoints.forEach(endpoint => {
            endpoint.tags.forEach(tag => {
                const parts = tag.split(separator);
                let current = groups;

                parts.forEach((part, index) => {
                    if (!current[part]) {
                        current[part] = {
                            name: part,
                            fullPath: parts.slice(0, index + 1).join(separator),
                            children: {},
                            endpoints: [],
                            count: 0
                        };
                    }

                    if (index === parts.length - 1) {
                        current[part].endpoints.push(endpoint);
                    }

                    current = current[part].children;
                });
            });
        });

        // Calculate counts
        this.calculateCounts(groups);

        return groups;
    }

    calculateCounts(groups) {
        Object.values(groups).forEach(group => {
            group.count = group.endpoints.length;
            if (Object.keys(group.children).length > 0) {
                this.calculateCounts(group.children);
                group.count += Object.values(group.children).reduce((sum, child) => sum + child.count, 0);
            }
        });
    }

    generateTreeViewHTML(groups, level = 0) {
        let html = '';

        Object.values(groups).forEach(group => {
            const hasChildren = Object.keys(group.children).length > 0;
            const isExpanded = this.config.treeView.expandByDefault;

            html += `
                <div class="swagger-tree-node" data-level="${level}">
                    <div class="swagger-tree-node-header" data-path="${group.fullPath}">
                        ${hasChildren ? `
                            <span class="swagger-tree-node-toggle">${isExpanded ? '▼' : '▶'}</span>
                        ` : ''}
                        <span class="swagger-tree-node-title">${group.name}</span>
                        ${this.config.treeView.showEndpointCount ? `
                            <span class="swagger-tree-node-count">${group.count}</span>
                        ` : ''}
                    </div>
                    ${hasChildren ? `
                        <div class="swagger-tree-node-children ${isExpanded ? '' : 'collapsed'}">
                            ${this.generateTreeViewHTML(group.children, level + 1)}
                        </div>
                    ` : ''}
                </div>
            `;
        });

        return html;
    }

    bindTreeViewEvents() {
        document.querySelectorAll('.swagger-tree-node-header').forEach(header => {
            header.addEventListener('click', (e) => {
                const children = header.nextElementSibling;
                if (children && children.classList.contains('swagger-tree-node-children')) {
                    const toggle = header.querySelector('.swagger-tree-node-toggle');
                    const isCollapsed = children.classList.contains('collapsed');

                    children.classList.toggle('collapsed');
                    if (toggle) {
                        toggle.textContent = isCollapsed ? '▼' : '▶';
                    }
                }

                // Scroll to tag section
                const path = header.dataset.path;
                if (path) {
                    this.scrollToTag(path);
                }
            });
        });
    }

    scrollToTag(tagPath) {
        const tag = tagPath.split(this.config.treeView.tagSeparator).pop();
        const element = document.querySelector(`#operations-tag-${tag}`);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }

    setupDirectExecution() {
        // Replace "Try it out" with direct execution
        document.addEventListener('click', (e) => {
            const tryItButton = e.target.closest('.try-out__btn');
            if (tryItButton && !tryItButton.dataset.mythEnhanced) {
                this.enhanceEndpoint(tryItButton);
                tryItButton.dataset.mythEnhanced = 'true';
            }
        });

        // Monitor for new endpoints being added
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType === 1 && node.classList.contains('opblock')) {
                        setTimeout(() => this.enhanceEndpoint(node), 100);
                    }
                });
            });
        });

        observer.observe(document.querySelector('.swagger-ui'), {
            childList: true,
            subtree: true
        });
    }

    enhanceEndpoint(container) {
        const opblock = container.closest ? container.closest('.opblock') : container;
        if (!opblock || opblock.dataset.mythEnhanced) return;

        opblock.dataset.mythEnhanced = 'true';

        // Replace execute button
        const executeBtn = opblock.querySelector('.btn.execute');
        if (executeBtn) {
            this.replaceExecuteButton(executeBtn, opblock);
        }

        // Add JSON beautify functionality
        if (this.config.ui.enableJsonBeautify) {
            this.addJsonBeautify(opblock);
        }

        // Add cache controls
        if (this.config.cache.enablePersistence) {
            this.addCacheControls(opblock);
        }
    }

    replaceExecuteButton(originalBtn, opblock) {
        const method = this.getMethodFromOpblock(opblock);
        const methodTexts = {
            GET: '🔍 Fetch',
            POST: '📤 Create',
            PUT: '📝 Update',
            PATCH: '🔧 Modify',
            DELETE: '🗑️ Delete'
        };

        const newBtn = document.createElement('button');
        newBtn.className = 'swagger-execute-btn';
        newBtn.innerHTML = `
            <span class="btn-text">${methodTexts[method] || '▶ Execute'}</span>
            <span class="spinner" style="display: none;"></span>
        `;

        newBtn.addEventListener('click', async (e) => {
            e.preventDefault();
            await this.executeRequest(opblock, newBtn);
        });

        originalBtn.parentNode.replaceChild(newBtn, originalBtn);
    }

    getMethodFromOpblock(opblock) {
        const classes = Array.from(opblock.classList);
        const methodClass = classes.find(cls => cls.startsWith('opblock-'));
        return methodClass ? methodClass.replace('opblock-', '').toUpperCase() : 'GET';
    }

    async executeRequest(opblock, button) {
        if (button.classList.contains('loading')) return;

        const startTime = performance.now();

        try {
            // Validate required fields
            if (this.config.ui.validateRequiredFields && !this.validateRequiredFields(opblock)) {
                this.showToast('Please fill all required fields', 'error');
                return;
            }

            // Set loading state
            this.setButtonLoading(button, true);

            // Cache request data
            if (this.config.cache.enablePersistence) {
                this.cacheRequestData(opblock);
            }

            // Execute original request
            const originalExecuteBtn = opblock.querySelector('.btn.execute') ||
                opblock.querySelector('[class*="execute"]');

            if (originalExecuteBtn) {
                originalExecuteBtn.click();
            }

            // Wait for response and enhance it
            await this.waitForResponse(opblock, startTime);
        } catch (error) {
            console.error('Request execution failed:', error);
            this.showToast('Request failed: ' + error.message, 'error');
        } finally {
            this.setButtonLoading(button, false);
        }
    }

    validateRequiredFields(opblock) {
        const requiredInputs = opblock.querySelectorAll('input[required], select[required], textarea[required]');
        return Array.from(requiredInputs).every(input => input.value.trim() !== '');
    }

    setButtonLoading(button, isLoading) {
        const text = button.querySelector('.btn-text');
        const spinner = button.querySelector('.spinner');

        if (isLoading) {
            button.classList.add('loading');
            button.disabled = true;
            text.style.display = 'none';
            spinner.style.display = 'block';
        } else {
            button.classList.remove('loading');
            button.disabled = false;
            text.style.display = 'block';
            spinner.style.display = 'none';
        }
    }

    async waitForResponse(opblock, startTime) {
        return new Promise((resolve) => {
            const observer = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    mutation.addedNodes.forEach((node) => {
                        if (node.nodeType === 1 && node.classList.contains('responses-wrapper')) {
                            const endTime = performance.now();
                            this.enhanceResponse(node, endTime - startTime);
                            observer.disconnect();
                            resolve();
                        }
                    });
                });
            });

            observer.observe(opblock, { childList: true, subtree: true });

            // Fallback timeout
            setTimeout(() => {
                observer.disconnect();
                resolve();
            }, 30000);
        });
    }

    enhanceResponse(responseWrapper, timing) {
        if (this.config.performance.enableTiming) {
            this.addTimingInfo(responseWrapper, timing);
        }

        if (this.config.performance.enableStatusColors) {
            this.colorizeStatusCodes(responseWrapper);
        }
    }

    addTimingInfo(responseWrapper, timing) {
        const timingHtml = `
            <div class="swagger-timing-info">
                <div><strong>⏱️ Response Time:</strong> ${timing.toFixed(0)}ms</div>
                <div class="swagger-timing-breakdown">
                    <div class="swagger-timing-item">
                        <div class="swagger-timing-label">Total</div>
                        <div class="swagger-timing-value ${timing > 2000 ? 'swagger-timing-slow' : ''}">
                            ${timing.toFixed(0)}ms
                        </div>
                    </div>
                </div>
            </div>
        `;

        responseWrapper.insertAdjacentHTML('afterbegin', timingHtml);
    }

    colorizeStatusCodes(responseWrapper) {
        const statusElements = responseWrapper.querySelectorAll('.response-col_status');
        statusElements.forEach(element => {
            const status = element.textContent.trim();
            const statusCode = parseInt(status);

            if (statusCode >= 100 && statusCode < 200) {
                element.classList.add('status-1xx');
            } else if (statusCode >= 200 && statusCode < 300) {
                element.classList.add('status-2xx');
            } else if (statusCode >= 300 && statusCode < 400) {
                element.classList.add('status-3xx');
            } else if (statusCode >= 400 && statusCode < 500) {
                element.classList.add('status-4xx');
            } else if (statusCode >= 500) {
                element.classList.add('status-5xx');
            }
        });
    }

    setupModelCollapse() {
        const modelsSection = document.querySelector('.models');
        if (modelsSection) {
            const toggleHtml = `
                <div class="swagger-models-toggle" id="swagger-models-toggle">
                    📋 View Models & Schemas
                </div>
            `;

            modelsSection.insertAdjacentHTML('beforebegin', toggleHtml);
            modelsSection.classList.add('swagger-models-section', 'collapsed');

            document.getElementById('swagger-models-toggle').addEventListener('click', () => {
                modelsSection.classList.toggle('collapsed');
            });
        }
    }

    addJsonBeautify(opblock) {
        const textareas = opblock.querySelectorAll('textarea');
        textareas.forEach(textarea => {
            if (textarea.closest('.swagger-beautify-container')) return;

            const container = document.createElement('div');
            container.className = 'swagger-beautify-container';

            const beautifyBtn = document.createElement('button');
            beautifyBtn.className = 'swagger-beautify-btn';
            beautifyBtn.innerHTML = '✨ Beautify JSON';
            beautifyBtn.type = 'button';

            beautifyBtn.addEventListener('click', () => {
                try {
                    const parsed = JSON.parse(textarea.value);
                    textarea.value = JSON.stringify(parsed, null, 2);
                } catch (error) {
                    this.showToast('Invalid JSON format', 'error');
                }
            });

            textarea.parentNode.insertBefore(container, textarea);
            container.appendChild(textarea);
            container.appendChild(beautifyBtn);
        });
    }

    addCacheControls(opblock) {
        const executeContainer = opblock.querySelector('.execute-wrapper') ||
            opblock.querySelector('.btn-group');

        if (executeContainer && !executeContainer.querySelector('.swagger-cache-controls')) {
            const cacheHtml = `
                <div class="swagger-cache-controls">
                    <button class="swagger-cache-btn" data-action="load">📥 Load</button>
                    <button class="swagger-cache-btn" data-action="save">💾 Save</button>
                    <button class="swagger-cache-btn" data-action="clear">🗑️ Clear</button>
                </div>
            `;

            executeContainer.insertAdjacentHTML('beforeend', cacheHtml);

            // Bind cache events
            executeContainer.querySelectorAll('.swagger-cache-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    const action = btn.dataset.action;
                    this.handleCacheAction(action, opblock);
                });
            });
        }
    }

    handleCacheAction(action, opblock) {
        const path = this.getEndpointPath(opblock);
        const method = this.getMethodFromOpblock(opblock);
        const key = `${method}_${path}`;

        switch (action) {
            case 'load':
                this.loadCachedData(key, opblock);
                break;
            case 'save':
                this.saveCachedData(key, opblock);
                break;
            case 'clear':
                this.clearCachedData(key, opblock);
                break;
        }
    }

    getEndpointPath(opblock) {
        const pathElement = opblock.querySelector('.opblock-summary-path');
        return pathElement ? pathElement.textContent.trim() : '';
    }

    cacheRequestData(opblock) {
        const path = this.getEndpointPath(opblock);
        const method = this.getMethodFromOpblock(opblock);
        const key = `${method}_${path}`;

        const data = this.extractFormData(opblock);
        this.cache.set(key, data);
    }

    loadCachedData(key, opblock) {
        const data = this.cache.get(key);
        if (data) {
            this.populateFormData(opblock, data);
            this.showToast('Data loaded from cache', 'success');
        } else {
            this.showToast('No cached data found', 'error');
        }
    }

    saveCachedData(key, opblock) {
        const data = this.extractFormData(opblock);
        this.cache.set(key, data);
        this.showToast('Data saved to cache', 'success');
    }

    clearCachedData(key, opblock) {
        this.cache.remove(key);
        this.clearFormData(opblock);
        this.showToast('Cache cleared', 'success');
    }

    extractFormData(opblock) {
        const data = {};

        // Extract parameters
        opblock.querySelectorAll('input, select, textarea').forEach(input => {
            if (input.name || input.id) {
                data[input.name || input.id] = input.value;
            }
        });

        return data;
    }

    populateFormData(opblock, data) {
        Object.entries(data).forEach(([key, value]) => {
            const input = opblock.querySelector(`[name="${key}"], #${key}`);
            if (input) {
                input.value = value;
            }
        });
    }

    clearFormData(opblock) {
        opblock.querySelectorAll('input, select, textarea').forEach(input => {
            if (input.type !== 'submit' && input.type !== 'button') {
                input.value = '';
            }
        });
    }

    showToast(message, type = 'info') {
        if (!this.config.performance.showToasts) return;

        const toast = document.createElement('div');
        toast.className = `swagger-toast ${type} top-right`;
        toast.textContent = message;

        document.body.appendChild(toast);

        setTimeout(() => {
            toast.remove();
        }, 3000);
    }
}

// Supporting classes
class SwaggerCache {
    constructor(config) {
        this.config = config;
        this.prefix = config.keyPrefix || 'swagger_cache_';
    }

    set(key, value) {
        if (!this.config.enablePersistence) return;

        try {
            const data = {
                value,
                timestamp: Date.now(),
                expiration: this.config.expirationMinutes * 60 * 1000
            };
            localStorage.setItem(this.prefix + key, JSON.stringify(data));
        } catch (error) {
            console.warn('Failed to save to cache:', error);
        }
    }

    get(key) {
        if (!this.config.enablePersistence) return null;

        try {
            const item = localStorage.getItem(this.prefix + key);
            if (!item) return null;

            const data = JSON.parse(item);
            const now = Date.now();

            if (data.expiration > 0 && (now - data.timestamp) > data.expiration) {
                this.remove(key);
                return null;
            }

            return data.value;
        } catch (error) {
            console.warn('Failed to load from cache:', error);
            return null;
        }
    }

    remove(key) {
        try {
            localStorage.removeItem(this.prefix + key);
        } catch (error) {
            console.warn('Failed to remove from cache:', error);
        }
    }
}

class SwaggerSearch {
    constructor(config) {
        this.config = config;
        this.searchInput = null;
        this.searchResults = null;
        this.debounceTimer = null;
    }

    initialize() {
        this.searchInput = document.getElementById('swagger-search-input');
        this.searchResults = document.getElementById('swagger-search-results');

        if (this.searchInput) {
            this.searchInput.addEventListener('input', this.handleSearch.bind(this));
            this.searchInput.addEventListener('focus', this.showResults.bind(this));
            this.searchInput.addEventListener('blur', this.hideResults.bind(this));
        }

        // Click outside to hide results
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.swagger-search-container')) {
                this.hideResults();
            }
        });
    }

    handleSearch(e) {
        const query = e.target.value.trim();

        if (query.length < this.config.minSearchLength) {
            this.hideResults();
            return;
        }

        if (this.config.enableRealTime) {
            clearTimeout(this.debounceTimer);
            this.debounceTimer = setTimeout(() => {
                this.performSearch(query);
            }, this.config.debounceMs);
        }
    }

    performSearch(query) {
        const results = window.mythSwagger.endpoints.filter(endpoint => {
            const searchText = [
                endpoint.summary,
                endpoint.description,
                endpoint.path,
                endpoint.method,
                ...endpoint.tags
            ].join(' ').toLowerCase();

            return this.config.caseSensitive ?
                searchText.includes(query) :
                searchText.includes(query.toLowerCase());
        });

        this.displayResults(results, query);
    }

    displayResults(results, query) {
        if (!this.searchResults) return;

        if (results.length === 0) {
            this.searchResults.innerHTML = '<div class="swagger-search-result">No results found</div>';
        } else {
            this.searchResults.innerHTML = results.map(result => {
                const highlightedSummary = this.config.enableHighlighting ?
                    this.highlightText(result.summary, query) : result.summary;

                return `
                    <div class="swagger-search-result" data-path="${result.path}" data-method="${result.method}">
                        <div class="swagger-search-result-title">${highlightedSummary}</div>
                        <div>
                            <span class="swagger-search-result-method method-${result.method.toLowerCase()}">${result.method}</span>
                            ${result.path}
                        </div>
                    </div>
                `;
            }).join('');
        }

        this.showResults();
        this.bindResultClicks();
    }

    highlightText(text, query) {
        if (!text || !query) return text;

        const regex = new RegExp(`(${query})`, this.config.caseSensitive ? 'g' : 'gi');
        return text.replace(regex, '<span class="swagger-search-highlight">$1</span>');
    }

    bindResultClicks() {
        this.searchResults.querySelectorAll('.swagger-search-result').forEach(result => {
            result.addEventListener('click', () => {
                const path = result.dataset.path;
                const method = result.dataset.method;
                this.navigateToEndpoint(path, method);
                this.hideResults();
            });
        });
    }

    navigateToEndpoint(path, method) {
        const element = document.querySelector(`[data-path="${path}"][data-method="${method.toLowerCase()}"]`);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'center' });

            // Expand the operation if collapsed
            const opblock = element.closest('.opblock');
            if (opblock && !opblock.classList.contains('is-open')) {
                const summary = opblock.querySelector('.opblock-summary-control');
                if (summary) summary.click();
            }
        }
    }

    showResults() {
        if (this.searchResults) {
            this.searchResults.style.display = 'block';
        }
    }

    hideResults() {
        setTimeout(() => {
            if (this.searchResults) {
                this.searchResults.style.display = 'none';
            }
        }, 150);
    }
}

class SwaggerTheme {
    constructor(config) {
        this.config = config;
        this.currentTheme = this.config.defaultTheme;
    }

    initialize() {
        this.loadSavedTheme();
        this.applyTheme(this.currentTheme);
        this.setupThemeToggle();

        if (this.currentTheme === 'auto') {
            this.watchSystemTheme();
        }
    }

    loadSavedTheme() {
        if (this.config.persistPreference) {
            const saved = localStorage.getItem('swagger-theme');
            if (saved) {
                this.currentTheme = saved;
            }
        }
    }

    saveTheme(theme) {
        if (this.config.persistPreference) {
            localStorage.setItem('swagger-theme', theme);
        }
    }

    applyTheme(theme) {
        const root = document.documentElement;

        if (theme === 'auto') {
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            root.setAttribute('data-theme', prefersDark ? 'dark' : 'light');
        } else {
            root.setAttribute('data-theme', theme);
        }

        this.currentTheme = theme;
        this.saveTheme(theme);
    }

    setupThemeToggle() {
        const toggle = document.getElementById('swagger-theme-toggle');
        if (toggle && this.config.allowUserToggle) {
            toggle.addEventListener('click', () => {
                this.cycleTheme();
            });
        }
    }

    cycleTheme() {
        const themes = ['light', 'dark', 'auto'];
        const currentIndex = themes.indexOf(this.currentTheme);
        const nextIndex = (currentIndex + 1) % themes.length;

        this.applyTheme(themes[nextIndex]);

        // Update toggle icon
        const toggle = document.getElementById('swagger-theme-toggle');
        if (toggle) {
            const icons = { light: '☀️', dark: '🌙', auto: '🌓' };
            toggle.textContent = icons[this.currentTheme];
        }
    }

    watchSystemTheme() {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        mediaQuery.addListener(() => {
            if (this.currentTheme === 'auto') {
                this.applyTheme('auto');
            }
        });
    }
}

class SwaggerKeyboardShortcuts {
    constructor(config) {
        this.config = config;
        this.shortcuts = {
            'Ctrl+Enter': () => this.executeCurrentEndpoint(),
            'Ctrl+Delete': () => this.clearCurrentForm(),
            'Ctrl+F': () => this.focusSearch(),
            'Ctrl+Shift+T': () => window.mythSwagger.theme.cycleTheme(),
            'Ctrl+Shift+F': () => this.beautifyCurrentJson()
        };
    }

    initialize() {
        if (!this.config.enableKeyboardShortcuts) return;

        document.addEventListener('keydown', this.handleKeydown.bind(this));
        this.showKeyboardHints();
    }

    handleKeydown(e) {
        const key = this.getKeyString(e);
        const handler = this.shortcuts[key];

        if (handler) {
            e.preventDefault();
            handler();
        }
    }

    getKeyString(e) {
        const parts = [];
        if (e.ctrlKey) parts.push('Ctrl');
        if (e.shiftKey) parts.push('Shift');
        if (e.altKey) parts.push('Alt');
        parts.push(e.key === ' ' ? 'Space' : e.key);
        return parts.join('+');
    }

    executeCurrentEndpoint() {
        const activeEndpoint = document.querySelector('.opblock.is-open');
        if (activeEndpoint) {
            const executeBtn = activeEndpoint.querySelector('.swagger-execute-btn');
            if (executeBtn) executeBtn.click();
        }
    }

    clearCurrentForm() {
        const activeEndpoint = document.querySelector('.opblock.is-open');
        if (activeEndpoint) {
            window.mythSwagger.clearFormData(activeEndpoint);
        }
    }

    focusSearch() {
        const searchInput = document.getElementById('swagger-search-input');
        if (searchInput) searchInput.focus();
    }

    beautifyCurrentJson() {
        const activeTextarea = document.activeElement;
        if (activeTextarea && activeTextarea.tagName === 'TEXTAREA') {
            try {
                const parsed = JSON.parse(activeTextarea.value);
                activeTextarea.value = JSON.stringify(parsed, null, 2);
            } catch (error) {
                // Ignore invalid JSON
            }
        }
    }

    showKeyboardHints() {
        const hints = document.createElement('div');
        hints.className = 'swagger-keyboard-hint';
        hints.innerHTML = '💡 Press Ctrl+Enter to execute, Ctrl+F to search';
        document.body.appendChild(hints);

        // Show hints briefly on load
        setTimeout(() => hints.classList.add('show'), 1000);
        setTimeout(() => hints.classList.remove('show'), 4000);
    }
}

class SwaggerPerformance {
    constructor(config) {
        this.config = config;
        this.requestStartTimes = new Map();
    }

    initialize() {
        if (this.config.enableTiming) {
            this.interceptRequests();
        }
    }

    interceptRequests() {
        // This is a simplified version - in practice you'd need to hook into
        // the actual Swagger UI request mechanism
        const originalFetch = window.fetch;
        window.fetch = async (...args) => {
            const startTime = performance.now();
            try {
                const response = await originalFetch(...args);
                const endTime = performance.now();
                console.log(`Request completed in ${endTime - startTime}ms`);
                return response;
            } catch (error) {
                const endTime = performance.now();
                console.log(`Request failed in ${endTime - startTime}ms`);
                throw error;
            }
        };
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    // Wait for Swagger UI to be fully loaded
    setTimeout(() => {
        window.mythSwagger = new MythSwaggerUI(window.swaggerAdvancedConfig || {});
        window.mythSwagger.initialize();
    }, 1000);
});

// Export for external use
window.MythSwaggerUI = MythSwaggerUI;
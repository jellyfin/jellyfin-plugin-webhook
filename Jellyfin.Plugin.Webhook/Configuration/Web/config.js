define([
    "loading",
    "emby-input",
    "emby-button",
    "emby-collapse",
    "formDialogStyle",
    "flexStyles"
], function (loading) {
    const pluginId = "529397D0-A0AA-43DB-9537-7CFDE936C1E3";

    const configurationWrapper = document.querySelector("#configurationWrapper");
    const templateBase = document.querySelector("#template-base");

    const btnAddDiscord = document.querySelector("#btnAddDiscord");
    const templateDiscord = document.querySelector("#template-discord");
    const discordDefaultEmbedColor = "#AA5CC3";

    const btnAddGotify = document.querySelector("#btnAddGotify");
    const templateGotify = document.querySelector("#template-gotify");

    const btnAddPushover = document.querySelector("#btnAddPushover");
    const templatePushover = document.querySelector("#template-pushover");

    const btnAddGeneric = document.querySelector("#btnAddGeneric");
    const templateGeneric = document.querySelector("#template-generic");
    const templateGericValue = document.querySelector("#template-generic-value");

    // Add click handlers
    btnAddDiscord.addEventListener("click", addDiscordConfig);
    btnAddGotify.addEventListener("click", addGotifyConfig);
    btnAddPushover.addEventListener("click", addPushoverConfig);
    btnAddGeneric.addEventListener("click", addGenericConfig);
    document.querySelector("#saveConfig").addEventListener("click", saveConfig);

    /**
     * Adds the base config template.
     * @param template {HTMLElement} Inner template.
     * @param name {string} Config name.
     * @returns {HTMLElement} Wrapped template.
     */
    function addBaseConfig(template, name) {
        const collapse = document.createElement("div");
        collapse.setAttribute("is", "emby-collapse");
        collapse.setAttribute("title", name);
        collapse.dataset.configWrapper = "1";
        const collapseContent = document.createElement("div");
        collapseContent.classList.add("collapseContent");

        // Append template content.
        collapseContent.appendChild(template);

        // Append removal button.
        const btnRemove = document.createElement("button");
        btnRemove.innerText = "Remove";
        btnRemove.setAttribute("is", "emby-button");
        btnRemove.classList.add("raised", "button-warning", "block");
        btnRemove.addEventListener("click", removeConfig);

        collapseContent.appendChild(btnRemove);
        collapse.appendChild(collapseContent);

        return collapse;
    }

    /**
     * Adds discord configuration.
     * @param config {Object}
     */
    function addDiscordConfig(config) {
        const template = document.createElement("div");
        template.dataset.type = "discord";
        template.appendChild(templateBase.cloneNode(true).content);
        template.appendChild(templateDiscord.cloneNode(true).content);

        const txtColor = template.querySelector("[data-name=txtEmbedColor]");
        const selColor = template.querySelector("[data-name=EmbedColor]");
        txtColor.addEventListener("input", function () {
            selColor.value = this.value;
        });
        selColor.addEventListener("change", function () {
            txtColor.value = this.value;
        });

        const base = addBaseConfig(template, "Discord");
        configurationWrapper.appendChild(base);

        // Load configuration.
        setDiscordConfig(config, base);
    }

    /**
     * Adds gotify configuration.
     * @param config {Object}
     */
    function addGotifyConfig(config) {
        const template = document.createElement("div");
        template.dataset.type = "gotify";
        template.appendChild(templateBase.cloneNode(true).content);
        template.appendChild(templateGotify.cloneNode(true).content);

        const base = addBaseConfig(template, "Gotify");
        configurationWrapper.appendChild(base);

        // Load configuration.
        setGotifyConfig(config, base);
    }

    /**
     * Adds pushover configuration.
     * @param config {Object}
     */
    function addPushoverConfig(config) {
        const template = document.createElement("div");
        template.dataset.type = "pushover";
        template.appendChild(templateBase.cloneNode(true).content);
        template.appendChild(templatePushover.cloneNode(true).content);

        const base = addBaseConfig(template, "Pushover");
        configurationWrapper.appendChild(base);

        // Load configuration
        setPushoverConfig(config, base);
    }

    /**
     * Adds generic config.
     * @param config {Object}
     */
    function addGenericConfig(config) {
        const template = document.createElement("div");
        template.dataset.type = "generic";
        template.appendChild(templateBase.cloneNode(true).content);
        template.appendChild(templateGeneric.cloneNode(true).content);

        const base = addBaseConfig(template, "Generic");
        configurationWrapper.appendChild(base);

        // Load configuration
        setGenericConfig(config, base);
    }

    /**
     * Loads config into element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function setBaseConfig(config, element) {
        element.querySelector("[data-name=chkEnableMovies]").checked = config.EnableMovies || true;
        element.querySelector("[data-name=chkEnableEpisodes]").checked = config.EnableEpisodes || true;
        element.querySelector("[data-name=chkEnableSeasons]").checked = config.EnableSeasons || true;
        element.querySelector("[data-name=chkEnableSeries]").checked = config.EnableSeries || true;
        element.querySelector("[data-name=chkEnableAlbums]").checked = config.EnableAlbums || true;
        element.querySelector("[data-name=chkEnableSongs]").checked = config.EnableSongs || true;
        element.querySelector("[data-name=txtWebhookUri]").value = config.WebhookUri || "";
        element.querySelector("[data-name=txtTemplate]").value = atob(config.Template || "");
    }

    /**
     * Loads config into element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function setDiscordConfig(config, element) {
        setBaseConfig(config, element);

        element.querySelector("[data-name=txtAvatarUrl]").value = config.AvatarUrl || "";
        element.querySelector("[data-name=txtUsername]").value = config.Username || "";
        element.querySelector("[data-name=ddlMentionType]").value = config.MentionType || "None";
        element.querySelector("[data-name=txtEmbedColor]").value = config.EmbedColor || discordDefaultEmbedColor;
        element.querySelector("[data-name=EmbedColor]").value = config.EmbedColor || discordDefaultEmbedColor;
    }

    /**
     * Loads config into element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function setGotifyConfig(config, element) {
        setBaseConfig(config, element);

        element.querySelector("[data-name=txtToken]").value = config.Token || "";
        element.querySelector("[data-name=txtPriority]").value = config.Priority || 0;
    }

    /**
     * Loads config into element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function setPushoverConfig(config, element) {
        setBaseConfig(config, element);

        element.querySelector("[data-name=txtToken]").value = config.Token || "";
        element.querySelector("[data-name=txtUserToken]").value = config.UserToken || "";
        element.querySelector("[data-name=txtDevice]").value = config.Device || "";
        element.querySelector("[data-name=txtTitle]").value = config.Title || "";
        element.querySelector("[data-name=txtMessageUrl]").value = config.MessageUrl || "";
        element.querySelector("[data-name=txtMessageUrlTitle]").value = config.MessageUrlTitle || "";
        element.querySelector("[data-name=ddlMessagePriority]").value = config.MessagePriority || "";
        element.querySelector("[data-name=ddlNotificationSound]").value = config.NotificationSound || "";
    }

    /**
     * Loads config into element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function setGenericConfig(config, element) {
        setBaseConfig(config, element);

        for (let i = 0; i < config.Headers.length; i++) {
            addGenericConfigHeader(config.Headers[i], element);
        }

        for (let i = 0; i < config.Fields.length; i++) {
            addGenericConfigField(config.Fields[i], element);
        }
    }

    /**
     * Load the header into the element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function addGenericConfigHeader(config, element) {
        // TODO
    }

    /**
     * Load the field into the element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function addGenericConfigField(config, element) {
        // TODO
    }

    /**
     * Get base config.
     * @param element {HTMLElement}
     * @returns {Object} configuration result.
     */
    function getBaseConfig(element) {
        const config = {};

        config.EnableMovies = element.querySelector("[data-name=chkEnableMovies]").checked || false;
        config.EnableEpisodes = element.querySelector("[data-name=chkEnableEpisodes]").checked || false;
        config.EnableSeasons = element.querySelector("[data-name=chkEnableSeasons]").checked || false;
        config.EnableSeries = element.querySelector("[data-name=chkEnableSeries]").checked || false;
        config.EnableAlbums = element.querySelector("[data-name=chkEnableAlbums]").checked || false;
        config.EnableSongs = element.querySelector("[data-name=chkEnableSongs]").checked || false;
        config.WebhookUri = element.querySelector("[data-name=txtWebhookUri]").value || "";
        config.Template = btoa(element.querySelector("[data-name=txtTemplate]").value || "");

        return config;
    }

    /**
     * Get discord specific config.
     * @param e {HTMLElement}
     * @returns {Object} configuration result.
     */
    function getDiscordConfig(e) {
        const config = getBaseConfig(e);
        config.AvatarUrl = e.querySelector("[data-name=txtAvatarUrl]").value || "";
        config.Username = e.querySelector("[data-name=txtUsername]").value || "";
        config.MentionType = e.querySelector("[data-name=ddlMentionType]").value || "";
        config.EmbedColor = e.querySelector("[data-name=txtEmbedColor]").value || "";
        return config;
    }

    /**
     * Get gotify specific config.
     * @param e {HTMLElement}
     * @returns {Object} configuration result.
     */
    function getGotifyConfig(e) {
        const config = getBaseConfig(e);
        config.Token = e.querySelector("[data-name=txtToken]").value || "";
        config.Priority = e.querySelector("[data-name=txtPriority]").value || 0;
        return config;
    }

    /**
     * Get pushover specific config.
     * @param e {HTMLElement}
     * @returns {Object} configuration result.
     */
    function getPushoverConfig(e) {
        const config = getBaseConfig(e);
        config.Token = e.querySelector("[data-name=txtToken]").value || "";
        config.UserToken = e.querySelector("[data-name=txtUserToken]").value || "";
        config.Device = e.querySelector("[data-name=txtDevice]").value || "";
        config.Title = e.querySelector("[data-name=txtTitle]").value || "";
        config.MessageUrl = e.querySelector("[data-name=txtMessageUrl]").value || "";
        config.MessageUrlTitle = e.querySelector("[data-name=txtMessageUrlTitle]").value || "";
        config.MessagePriority = e.querySelector("[data-name=ddlMessagePriority]").value || 0;
        config.NotificationSound = e.querySelector("[data-name=ddlNotificationSound]").value || "";
        return config;
    }

    /**
     * Get generic specific config.
     * @param e {HTMLElement}
     * @return {Object} configuration result.
     */
    function getGenericConfig(e) {
        const config = getBaseConfig(e);
        // TODO get fields and headers.        
        return config;
    }

    /**
     * Removes config from dom.
     * @param e {Event}
     */
    function removeConfig(e) {
        e.preventDefault();
        findParentBySelector(e.target, '[data-config-wrapper]').remove();
    }

    function saveConfig(e) {
        e.preventDefault();

        loading.show();

        const config = {};
        config.ServerUrl = document.querySelector("#txtServerUrl").value;
        config.DiscordOptions = [];
        const discordConfigs = document.querySelectorAll("[data-type=discord]");
        for (let i = 0; i < discordConfigs.length; i++) {
            config.DiscordOptions.push(getDiscordConfig(discordConfigs[i]));
        }

        config.GotifyOptions = [];
        const gotifyConfigs = document.querySelectorAll("[data-type=gotify]");
        for (let i = 0; i < gotifyConfigs.length; i++) {
            config.GotifyOptions.push(getGotifyConfig(gotifyConfigs[i]));
        }

        config.PushoverOptions = [];
        const pushoverConfigs = document.querySelectorAll("[data-type=pushover]");
        for (let i = 0; i < pushoverConfigs.length; i++) {
            config.PushoverOptions.push(getPushoverConfig(pushoverConfigs[i]));
        }

        config.GenericOptions = [];
        const genericConfigs = document.querySelectorAll("[data-type=generic]");
        for (let i = 0; i < genericConfigs.length; i++) {
            config.GenericOptions.push(getGenericConfig(genericConfigs[i]));
        }

        ApiClient.updatePluginConfiguration(pluginId, config).then(Dashboard.processPluginConfigurationUpdateResult);
    }

    function loadConfig() {
        loading.show();

        ApiClient.getPluginConfiguration(pluginId).then(function (config) {
            document.querySelector("#txtServerUrl").value = config.ServerUrl || "";
            for (let i = 0; i < config.DiscordOptions.length; i++) {
                addDiscordConfig(config.DiscordOptions[i]);
            }

            for (let i = 0; i < config.GotifyOptions.length; i++) {
                addGotifyConfig(config.GotifyOptions[i]);
            }

            for (let i = 0; i < config.PushoverOptions.length; i++) {
                addPushoverConfig(config.PushoverOptions[i]);
            }

            for (let i = 0; i < config.GenericOptions.length; i++) {
                addGenericConfig(config.GenericOptions[i]);
            }
        });

        loading.hide();
    }

    loadConfig();


    /*** Utils ***/
    function collectionHas(a, b) {
        for (let i = 0; i < a.length; i++) {
            if (a[i] === b) {
                return true;
            }
        }
        return false;
    }

    /**
     *
     * @param elm {EventTarget}
     * @param selector {string}
     * @returns {HTMLElement}
     */
    function findParentBySelector(elm, selector) {
        const all = document.querySelectorAll(selector);
        let cur = elm.parentNode;
        //keep going up until you find a match
        while (cur && !collectionHas(all, cur)) {
            cur = cur.parentNode;
        }
        return cur;
    }
});
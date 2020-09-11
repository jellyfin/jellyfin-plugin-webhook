define([
    "loading",
    "dialogHelper",
    "emby-input",
    "emby-button",
    "emby-collapse",
    "formDialogStyle",
    "flexStyles"
], function (loading, dialogHelper) {
    var pluginId = "529397D0-A0AA-43DB-9537-7CFDE936C1E3";
    var discordDefaultEmbedColor = "#AA5CC3";
    var defaultDiscordTemplate = `{
  "content": "{{MentionType}}",
  "avatar_url": "{{AvatarUrl}}",
  "username": "{{Username}}",
  "embeds": [
\t{
\t\t"color": "{{EmbedColor}}",
\t\t"footer": {
\t\t\t"text": "From {{ServerName}}",
\t\t\t"iconUrl": "{{AvatarUrl}}"
\t\t},
        {{if_equals ItemType 'Season'}}
            "title": "{{SeriesName}} {{Name}} has been added to {{ServerName}}",
        {{else}}
        {{if_equals ItemType 'Episode'}}
            "title": "{{SeriesName}} S{{SeasonNumber}}E{{EpisodeNumber}} {{Name}} has been added to {{ServerName}}",
        {{else}}
            "title": "{{Name}} ({{Year}}) has been added to {{ServerName}}",        
        {{/if_equals}}       
        {{/if_equals}}
        "url": "{{ServerUrl}}/web/index.html#!/details?id={{ItemId}}&serverId={{ServerId}}",
        "thumbnail":{
            "url": "{{ServerUrl}}/Items/{{ItemId}}/Images/Primary"
        }
\t}
  ]
}`;
        
    var configurationWrapper = document.querySelector("#configurationWrapper");
    var templateBase = document.querySelector("#template-base");
    
    var btnAddDiscord = document.querySelector("#btnAddDiscord");
    var templateDiscord = document.querySelector("#template-discord");
    
    // Add click handlers
    btnAddDiscord.addEventListener("click", addDiscordConfig);
    document.querySelector("#saveConfig").addEventListener("click", saveConfig);

    /**
     * Adds the base config template.
     * @param template {HTMLElement} Inner template.
     * @param name {string} Config name.
     * @returns {HTMLElement} Wrapped template.
     */
    function addBaseConfig(template, name){
        var collapse = document.createElement("div");
        collapse.setAttribute("is", "emby-collapse");
        collapse.setAttribute("title", name);
        collapse.dataset.configWrapper = "1";
        var collapseContent = document.createElement("div");
        collapseContent.classList.add("collapseContent");

        // Append template content.
        collapseContent.appendChild(template);
        
        // Append removal button.
        var btnRemove = document.createElement("button");
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
    function addDiscordConfig(config){
        var template = document.createElement("div");
        template.dataset.type = "discord";
        template.appendChild(templateBase.cloneNode(true).content);
        template.appendChild(templateDiscord.cloneNode(true).content);
        
        var txtColor = template.querySelector("[data-name=txtEmbedColor]");
        var selColor = template.querySelector("[data-name=EmbedColor]");
        txtColor.addEventListener("input", function(){ selColor.value = this.value; });
        selColor.addEventListener("change", function(){ txtColor.value = this.value; });

        var base = addBaseConfig(template, "Discord");
        configurationWrapper.appendChild(base);
        
        // Load configuration.
        setDiscordConfig(config, base);
    }

    /**
     * Loads config into element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function setBaseConfig(config, element){
        element.querySelector("[data-name=chkEnableMovies]").checked = config.EnableMovies || true;
        element.querySelector("[data-name=chkEnableEpisodes]").checked = config.EnableEpisodes || true;
        element.querySelector("[data-name=chkEnableSeasons]").checked = config.EnableSeasons || true;
        element.querySelector("[data-name=chkEnableSeries]").checked = config.EnableSeries || true;
        element.querySelector("[data-name=chkEnableAlbums]").checked = config.EnableAlbums || true;
        element.querySelector("[data-name=chkEnableSongs]").checked = config.EnableSongs || true;
        element.querySelector("[data-name=txtWebhookUri]").value = config.WebhookUri || "";
        element.querySelector("[data-name=txtTemplate]").value = atob(config.Template || "") || defaultDiscordTemplate;
    }
    
    /**
     * Loads config into element.
     * @param config {Object}
     * @param element {HTMLElement}
     */
    function setDiscordConfig(config, element){
        setBaseConfig(config, element);

        element.querySelector("[data-name=txtAvatarUrl]").value = config.AvatarUrl || "";
        element.querySelector("[data-name=txtUsername]").value = config.Username || "";
        element.querySelector("[data-name=ddlMentionType]").value = config.MentionType || "None";
        element.querySelector("[data-name=txtEmbedColor]").value = config.EmbedColor || discordDefaultEmbedColor;
        element.querySelector("[data-name=EmbedColor]").value = config.EmbedColor || discordDefaultEmbedColor;
    }
    
    /**
     * Get base config.
     * @param element {HTMLElement}
     * @returns {Object} configuration result.
     */
    function getBaseConfig(element){
        var config = {};
        
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
    function getDiscordConfig(e){
        var config = getBaseConfig(e);
        config.AvatarUrl = e.querySelector("[data-name=txtAvatarUrl]").value || "";
        config.Username = e.querySelector("[data-name=txtUsername]").value || "";
        config.MentionType = e.querySelector("[data-name=ddlMentionType]").value || "";
        config.EmbedColor = e.querySelector("[data-name=txtEmbedColor]").value || "";
        return config;
    }

    /**
     * Removes config from dom.
     * @param e {Event}
     */
    function removeConfig(e){
        e.preventDefault();
        console.log(e);
        findParentBySelector(e.target, '[data-config-wrapper]').remove();
    }     
    
    function saveConfig(e){
        e.preventDefault();
        
        loading.show();
        
        var config = {};
        config.ServerUrl = document.querySelector("#txtServerUrl").value;
        config.DiscordOptions = [];
        var discordConfigs = document.querySelectorAll("[data-type=discord]");
        for(var i = 0; i < discordConfigs.length; i++){
            config.DiscordOptions.push(getDiscordConfig(discordConfigs[i]));
        }

        console.log(config);
        ApiClient.updatePluginConfiguration(pluginId, config).then(Dashboard.processPluginConfigurationUpdateResult);
    }
    
    function loadConfig(){
        loading.show();

        ApiClient.getPluginConfiguration(pluginId).then(function (config) {
           document.querySelector("#txtServerUrl").value = config.ServerUrl || "";
           for(var i = 0; i < config.DiscordOptions.length; i++){
               addDiscordConfig(config.DiscordOptions[i]);
           }           
        });

        loading.hide();
    }
    
    loadConfig();
    
    
    /*** Utils ***/
    function collectionHas(a, b) {
        for (let i = 0, len = a.length; i < len; i++) {
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
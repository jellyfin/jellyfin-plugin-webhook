export default function (view) {
    /*** Utils ***/
    /**
     * Determine if a collection contains an object.
     * @param a {Array}
     * @param b {Object}
     * @return {boolean}
     */
    const collectionHas = function (a, b) {
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
    const findParentBySelector = function (elm, selector) {
        const all = document.querySelectorAll(selector);
        let cur = elm.parentNode;
        //keep going up until you find a match
        while (cur && !collectionHas(all, cur)) {
            cur = cur.parentNode;
        }
        return cur;
    }

    const Webhook = {
        pluginId: "71552A5A-5C5C-4350-A2AE-EBE451A30173",

        configurationWrapper: document.querySelector("#configurationWrapper"),

        notificationType: {
            template: document.querySelector("#template-notification-type"),
            values: {
                "ItemAdded": "Item Added",
                "ItemDeleted": "Item Deleted",
                "PlaybackStart": "Playback Start",
                "PlaybackProgress": "Playback Progress",
                "PlaybackStop": "Playback Stop",
                "SubtitleDownloadFailure": "Subtitle Download Failure",
                "AuthenticationFailure": "Authentication Failure",
                "AuthenticationSuccess": "Authentication Success",
                "SessionStart": "Session Start",
                "PendingRestart": "Pending Restart",
                "TaskCompleted": "Task Completed",
                "PluginInstallationCancelled": "Plugin Installation Cancelled",
                "PluginInstallationFailed": "Plugin Installation Failed",
                "PluginInstalled": "Plugin Installed",
                "PluginInstalling": "Plugin Installing",
                "PluginUninstalled": "Plugin Uninstalled",
                "PluginUpdated": "Plugin Updated",
                "UserCreated": "User Created",
                "UserDeleted": "User Deleted",
                "UserLockedOut": "User Locked Out",
                "UserPasswordChanged": "User Password Changed",
                "UserUpdated": "User Updated",
                "UserDataSaved": "User Data Saved"
            },
            create: function (container, selected = []) {
                const notificationTypeKeys = Object.keys(Webhook.notificationType.values).sort();
                for (const key of notificationTypeKeys) {
                    const template = Webhook.notificationType.template.cloneNode(true).content;
                    const name = template.querySelector("[data-name=notificationTypeName]");
                    const value = template.querySelector("[data-name=notificationTypeValue]");

                    name.innerText = Webhook.notificationType.values[key];
                    value.dataset.value = key;
                    value.checked = selected.includes(key);

                    container.appendChild(template);
                }
            },
            get: function (container) {
                const notificationTypes = [];
                const checkboxes = container.querySelectorAll('[data-name=notificationTypeValue]:checked');
                for (const checkbox of checkboxes) {
                    notificationTypes.push(checkbox.dataset.value);
                }
                return notificationTypes;
            }
        },
        userFilter: {
            template: document.querySelector("#template-user-filter"),
            users: [],
            populate: async function () {
                const users = await window.ApiClient.getUsers();

                Webhook.userFilter.users = [];
                for (const user of users) {
                    Webhook.userFilter.users.push({
                        id: user.Id,
                        name: user.Name
                    });
                }
            },
            create: function (container, selected = []) {
                for (const user of Webhook.userFilter.users) {
                    const template = Webhook.userFilter.template.cloneNode(true).content;
                    const name = template.querySelector("[data-name=userFilterName]");
                    const value = template.querySelector("[data-name=userFilterValue]");

                    name.innerText = user.name;
                    value.dataset.value = user.id;
                    value.checked = selected.includes(user.id);

                    container.appendChild(template);
                }
            },
            get: function (container) {
                const userFilter = [];
                const checkboxes = container.querySelectorAll('[data-name=userFilterValue]:checked');
                for (const checkbox of checkboxes) {
                    userFilter.push(checkbox.dataset.value);
                }
                return userFilter;
            }
        },
        baseConfig: {
            template: document.querySelector("#template-base"),
            addConfig: function (template, destinationType, destinationName) {
                const collapse = document.createElement("div");
                collapse.setAttribute("is", "emby-collapse");

                if (destinationName) {
                    collapse.setAttribute("title", `${destinationName} - ${destinationType}`);
                } else {
                    collapse.setAttribute("title", destinationType);
                }
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
                btnRemove.addEventListener("click", Webhook.removeConfig);

                collapseContent.appendChild(btnRemove);
                collapse.appendChild(collapseContent);

                return collapse;
            },
            setConfig: function (config, element) {
                element.querySelector("[data-name=chkEnableMovies]").checked = config.EnableMovies || (typeof config.EnableMovies == "undefined");
                element.querySelector("[data-name=chkEnableEpisodes]").checked = config.EnableEpisodes || (typeof config.EnableEpisodes == "undefined");
                element.querySelector("[data-name=chkEnableSeasons]").checked = config.EnableSeasons || (typeof config.EnableSeasons == "undefined");
                element.querySelector("[data-name=chkEnableSeries]").checked = config.EnableSeries || (typeof config.EnableSeries == "undefined");
                element.querySelector("[data-name=chkEnableAlbums]").checked = config.EnableAlbums || (typeof config.EnableAlbums == "undefined");
                element.querySelector("[data-name=chkEnableSongs]").checked = config.EnableSongs || (typeof config.EnableSongs == "undefined");
                element.querySelector("[data-name=chkEnableVideos]").checked = config.EnableVideos || (typeof config.EnableVideos == "undefined");
                element.querySelector("[data-name=txtWebhookName]").value = config.WebhookName || "";
                element.querySelector("[data-name=txtWebhookUri]").value = config.WebhookUri || "";
                element.querySelector("[data-name=chkSendAllProperties]").checked = config.SendAllProperties || false;
                element.querySelector("[data-name=chkTrimWhitespace]").checked = config.TrimWhitespace || false;
                element.querySelector("[data-name=chkSkipEmptyMessageBody]").checked = config.SkipEmptyMessageBody || false;
                element.querySelector("[data-name=chkEnableWebhook]").checked = config.EnableWebhook !== undefined ? config.EnableWebhook : true;
                element.querySelector("[data-name=txtTemplate]").value = Webhook.atou(config.Template || "");

                const notificationTypeContainer = element.querySelector("[data-name=notificationTypeContainer]");
                Webhook.notificationType.create(notificationTypeContainer, config.NotificationTypes);

                const userFilterContainer = element.querySelector("[data-name=userFilterContainer]");
                Webhook.userFilter.create(userFilterContainer, config.UserFilter);
            },
            getConfig: function (element) {
                const config = {};

                config.EnableMovies = element.querySelector("[data-name=chkEnableMovies]").checked || false;
                config.EnableEpisodes = element.querySelector("[data-name=chkEnableEpisodes]").checked || false;
                config.EnableSeasons = element.querySelector("[data-name=chkEnableSeasons]").checked || false;
                config.EnableSeries = element.querySelector("[data-name=chkEnableSeries]").checked || false;
                config.EnableAlbums = element.querySelector("[data-name=chkEnableAlbums]").checked || false;
                config.EnableSongs = element.querySelector("[data-name=chkEnableSongs]").checked || false;
                config.EnableVideos = element.querySelector("[data-name=chkEnableVideos]").checked || false;
                config.WebhookName = element.querySelector("[data-name=txtWebhookName]").value || "";
                config.WebhookUri = element.querySelector("[data-name=txtWebhookUri]").value || "";
                config.SendAllProperties = element.querySelector("[data-name=chkSendAllProperties]").checked || false;
                config.TrimWhitespace = element.querySelector("[data-name=chkTrimWhitespace]").checked || false;
                config.SkipEmptyMessageBody = element.querySelector("[data-name=chkSkipEmptyMessageBody]").checked || false;
                config.EnableWebhook = element.querySelector("[data-name=chkEnableWebhook]").checked;
                config.Template = Webhook.utoa(element.querySelector("[data-name=txtTemplate]").value || "");

                config.NotificationTypes = [];
                config.NotificationTypes = Webhook.notificationType.get(element);

                config.UserFilter = [];
                config.UserFilter = Webhook.userFilter.get(element);
                return config;
            }
        },
        discord: {
            btnAdd: document.querySelector("#btnAddDiscord"),
            template: document.querySelector("#template-discord"),
            defaultEmbedColor: "#AA5CC3",
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "discord";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.discord.template.cloneNode(true).content);

                const txtColor = template.querySelector("[data-name=txtEmbedColor]");
                const selColor = template.querySelector("[data-name=EmbedColor]");
                txtColor.addEventListener("input", function () {
                    selColor.value = value;
                });
                selColor.addEventListener("change", function () {
                    txtColor.value = value;
                });

                const baseConfig = Webhook.baseConfig.addConfig(template, "Discord", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);

                // Load configuration.
                Webhook.discord.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                element.querySelector("[data-name=txtAvatarUrl]").value = config.AvatarUrl || "";
                element.querySelector("[data-name=txtUsername]").value = config.Username || "";
                element.querySelector("[data-name=ddlMentionType]").value = config.MentionType || "None";
                element.querySelector("[data-name=txtEmbedColor]").value = config.EmbedColor || Webhook.discord.defaultEmbedColor;
                element.querySelector("[data-name=EmbedColor]").value = config.EmbedColor || Webhook.discord.defaultEmbedColor;
            },
            getConfig: function (e) {
                const config = Webhook.baseConfig.getConfig(e);
                config.AvatarUrl = e.querySelector("[data-name=txtAvatarUrl]").value || "";
                config.Username = e.querySelector("[data-name=txtUsername]").value || "";
                config.MentionType = e.querySelector("[data-name=ddlMentionType]").value || "";
                config.EmbedColor = e.querySelector("[data-name=txtEmbedColor]").value || "";
                return config;
            }
        },
        generic: {
            btnAdd: document.querySelector("#btnAddGeneric"),
            template: document.querySelector("#template-generic"),
            templateGenericValue: document.querySelector("#template-generic-value"),
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "generic";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.generic.template.cloneNode(true).content);

                const baseConfig = Webhook.baseConfig.addConfig(template, "Generic", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);
                template.querySelector("[data-name=btnAddHeader]").addEventListener("click", function () {
                    Webhook.generic.addHeader(baseConfig, {});
                });
                template.querySelector("[data-name=btnAddField]").addEventListener("click", function () {
                    Webhook.generic.addField(baseConfig, {});
                });

                // Load configuration
                Webhook.generic.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                if (config.Headers) {
                    for (let i = 0; i < config.Headers.length; i++) {
                        Webhook.generic.addHeader(element, config.Headers[i]);
                    }
                }

                if (config.Fields) {
                    for (let i = 0; i < config.Fields.length; i++) {
                        Webhook.generic.addField(element, config.Fields[i]);
                    }
                }
            },
            getConfig: function (e) {
                const config = Webhook.baseConfig.getConfig(e);

                config.Fields = [];
                const fieldValues = e.querySelector("[data-name=field-wrapper]")
                    .querySelectorAll("[data-name=value]");
                for (let i = 0; i < fieldValues.length; i++) {
                    const field = {
                        Key: fieldValues[i].querySelector("[data-name=txtKey]").value,
                        Value: fieldValues[i].querySelector("[data-name=txtValue]").value
                    };

                    if (field.Key !== "" && field.Value !== "") {
                        config.Fields.push(field);
                    }
                }

                config.Headers = [];
                const headerValues = e.querySelector("[data-name=header-wrapper]")
                    .querySelectorAll("[data-name=value]");
                for (let i = 0; i < headerValues.length; i++) {
                    const header = {
                        Key: headerValues[i].querySelector("[data-name=txtKey]").value,
                        Value: headerValues[i].querySelector("[data-name=txtValue]").value
                    };
                    if (header.Key !== "" && header.Value !== "") {
                        config.Headers.push(header);
                    }
                }

                return config;
            },
            addHeader: function (element, config) {
                const template = document.createElement("div");
                template.appendChild(Webhook.generic.templateGenericValue.cloneNode(true).content);

                template.querySelector("[data-name=txtKey]").value = config.Key || "";
                template.querySelector("[data-name=txtValue]").value = config.Value || "";

                element.querySelector("[data-name=header-wrapper]").appendChild(template);
            },
            addField: function (element, config) {
                const template = document.createElement("div");
                template.appendChild(Webhook.generic.templateGenericValue.cloneNode(true).content);

                template.querySelector("[data-name=txtKey]").value = config.Key || "";
                template.querySelector("[data-name=txtValue]").value = config.Value || "";

                element.querySelector("[data-name=field-wrapper]").appendChild(template);
            }
        },
        genericForm: {
            btnAdd: document.querySelector("#btnAddGenericForm"),
            template: document.querySelector("#template-generic-form"),
            templateGenericValue: document.querySelector("#template-generic-form-value"),
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "generic-form";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.genericForm.template.cloneNode(true).content);

                const baseConfig = Webhook.baseConfig.addConfig(template, "GenericForm", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);
                template.querySelector("[data-name=btnAddHeader]").addEventListener("click", function () {
                    Webhook.genericForm.addHeader(baseConfig, {});
                });
                template.querySelector("[data-name=btnAddField]").addEventListener("click", function () {
                    Webhook.genericForm.addField(baseConfig, {});
                });

                // Load configuration
                Webhook.genericForm.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                if (config.Headers) {
                    for (let i = 0; i < config.Headers.length; i++) {
                        Webhook.genericForm.addHeader(element, config.Headers[i]);
                    }
                }

                if (config.Fields) {
                    for (let i = 0; i < config.Fields.length; i++) {
                        Webhook.genericForm.addField(element, config.Fields[i]);
                    }
                }
            },
            getConfig: function (e) {
                const config = Webhook.baseConfig.getConfig(e);

                config.Fields = [];
                const fieldValues = e.querySelector("[data-name=field-wrapper]")
                    .querySelectorAll("[data-name=value]");
                for (let i = 0; i < fieldValues.length; i++) {
                    const field = {
                        Key: fieldValues[i].querySelector("[data-name=txtKey]").value,
                        Value: fieldValues[i].querySelector("[data-name=txtValue]").value
                    };

                    if (field.Key !== "" && field.Value !== "") {
                        config.Fields.push(field);
                    }
                }

                config.Headers = [];
                const headerValues = e.querySelector("[data-name=header-wrapper]")
                    .querySelectorAll("[data-name=value]");
                for (let i = 0; i < headerValues.length; i++) {
                    const header = {
                        Key: headerValues[i].querySelector("[data-name=txtKey]").value,
                        Value: headerValues[i].querySelector("[data-name=txtValue]").value
                    };
                    if (header.Key !== "" && header.Value !== "") {
                        config.Headers.push(header);
                    }
                }

                return config;
            },
            addHeader: function (element, config) {
                const template = document.createElement("div");
                template.appendChild(Webhook.genericForm.templateGenericValue.cloneNode(true).content);

                template.querySelector("[data-name=txtKey]").value = config.Key || "";
                template.querySelector("[data-name=txtValue]").value = config.Value || "";

                element.querySelector("[data-name=header-wrapper]").appendChild(template);
            },
            addField: function (element, config) {
                const template = document.createElement("div");
                template.appendChild(Webhook.genericForm.templateGenericValue.cloneNode(true).content);

                template.querySelector("[data-name=txtKey]").value = config.Key || "";
                template.querySelector("[data-name=txtValue]").value = config.Value || "";

                element.querySelector("[data-name=field-wrapper]").appendChild(template);
            }
        },
        gotify: {
            btnAdd: document.querySelector("#btnAddGotify"),
            template: document.querySelector("#template-gotify"),
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "gotify";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.gotify.template.cloneNode(true).content);

                const baseConfig = Webhook.baseConfig.addConfig(template, "Gotify", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);

                // Load configuration.
                Webhook.gotify.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                element.querySelector("[data-name=txtToken]").value = config.Token || "";
                element.querySelector("[data-name=txtPriority]").value = config.Priority || 0;
            },
            getConfig: function (e) {
                const config = Webhook.baseConfig.getConfig(e);
                config.Token = e.querySelector("[data-name=txtToken]").value || "";
                config.Priority = e.querySelector("[data-name=txtPriority]").value || 0;
                return config;
            }
        },
        pushbullet: {
            btnAdd: document.querySelector("#btnAddPushbullet"),
            template: document.querySelector("#template-pushbullet"),
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "pushbullet";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.pushbullet.template.cloneNode(true).content);

                const baseConfig = Webhook.baseConfig.addConfig(template, "Pushbullet", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);

                // Load configuration.
                Webhook.pushbullet.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                element.querySelector("[data-name=txtToken]").value = config.Token || "";
                element.querySelector("[data-name=txtDeviceId]").value = config.DeviceId || "";
                element.querySelector("[data-name=txtChannel]").value = config.Channel || "";
            },
            getConfig: function (e) {
                const config = Webhook.baseConfig.getConfig(e);
                config.Token = e.querySelector("[data-name=txtToken]").value || "";
                config.DeviceId = e.querySelector("[data-name=txtDeviceId]").value || "";
                config.Channel = e.querySelector("[data-name=txtChannel]").value || "";
                return config;
            }
        },
        pushover: {
            btnAdd: document.querySelector("#btnAddPushover"),
            template: document.querySelector("#template-pushover"),
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "pushover";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.pushover.template.cloneNode(true).content);

                const baseConfig = Webhook.baseConfig.addConfig(template, "Pushover", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);

                // Load configuration
                Webhook.pushover.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                element.querySelector("[data-name=txtToken]").value = config.Token || "";
                element.querySelector("[data-name=txtUserToken]").value = config.UserToken || "";
                element.querySelector("[data-name=txtDevice]").value = config.Device || "";
                element.querySelector("[data-name=txtTitle]").value = config.Title || "";
                element.querySelector("[data-name=txtMessageUrl]").value = config.MessageUrl || "";
                element.querySelector("[data-name=txtMessageUrlTitle]").value = config.MessageUrlTitle || "";
                element.querySelector("[data-name=ddlMessagePriority]").value = config.MessagePriority || "";
                element.querySelector("[data-name=ddlNotificationSound]").value = config.NotificationSound || "";
            },
            getConfig: function (e) {
                const config = Webhook.baseConfig.getConfig(e);
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
        },
        slack: {
            btnAdd: document.querySelector("#btnAddSlack"),
            template: document.querySelector("#template-slack"),
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "slack";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.slack.template.cloneNode(true).content);

                const baseConfig = Webhook.baseConfig.addConfig(template, "Slack", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);

                // Load configuration.
                Webhook.slack.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                element.querySelector("[data-name=txtUsername]").value = config.Username || "";
                element.querySelector("[data-name=txtIconUrl]").value = config.IconUrl || "";
            },
            getConfig: function (e) {
                const config = Webhook.baseConfig.getConfig(e);
                config.Username = e.querySelector("[data-name=txtUsername]").value || "";
                config.IconUrl = e.querySelector("[data-name=txtIconUrl]").value || "";
                return config;
            }
        },
        smtp: {
            btnAdd: document.querySelector("#btnAddSmtp"),
            template: document.querySelector("#template-smtp"),
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "smtp";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.smtp.template.cloneNode(true).content);

                const baseConfig = Webhook.baseConfig.addConfig(template, "SMTP", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);

                // Load configuration
                Webhook.smtp.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                element.querySelector("[data-name=txtSenderAddress]").value = config.SenderAddress || "";
                element.querySelector("[data-name=txtReceiverAddress]").value = config.ReceiverAddress || "";
                element.querySelector("[data-name=txtSmtpServer]").value = config.SmtpServer || "";
                element.querySelector("[data-name=txtSmtpPort]").value = config.SmtpPort || "";
                element.querySelector("[data-name=chkUseCredentials]").checked = config.UseCredentials || false;
                element.querySelector("[data-name=txtUsername]").value = config.Username || "";
                element.querySelector("[data-name=txtPassword]").value = config.Password || "";
                element.querySelector("[data-name=chkUseSsl]").checked = config.UseSsl || false;
                element.querySelector("[data-name=chkIsHtml]").checked = config.IsHtml || false;
                element.querySelector("[data-name=txtSubjectTemplate]").value = Webhook.atou(config.SubjectTemplate || "");
            },
            getConfig: function (element) {
                const config = Webhook.baseConfig.getConfig(element);
                config.SenderAddress = element.querySelector("[data-name=txtSenderAddress]").value || "";
                config.ReceiverAddress = element.querySelector("[data-name=txtReceiverAddress]").value || "";
                config.SmtpServer = element.querySelector("[data-name=txtSmtpServer]").value || "";
                config.SmtpPort = element.querySelector("[data-name=txtSmtpPort]").value || "";
                config.UseCredentials = element.querySelector("[data-name=chkUseCredentials]").checked || false;
                config.Username = element.querySelector("[data-name=txtUsername]").value || "";
                config.Password = element.querySelector("[data-name=txtPassword]").value || "";
                config.UseSsl = element.querySelector("[data-name=chkUseSsl]").checked || false;
                config.IsHtml = element.querySelector("[data-name=chkIsHtml]").checked || false;
                config.SubjectTemplate = Webhook.utoa(element.querySelector("[data-name=txtSubjectTemplate]").value || "");
                return config;
            }
        },
        mqtt: {
            btnAdd: document.querySelector("#btnAddMqtt"),
            template: document.querySelector("#template-mqtt"),
            addConfig: function (config) {
                const template = document.createElement("div");
                template.dataset.type = "mqtt";
                template.appendChild(Webhook.baseConfig.template.cloneNode(true).content);
                template.appendChild(Webhook.mqtt.template.cloneNode(true).content);

                const baseConfig = Webhook.baseConfig.addConfig(template, "MQTT", config.WebhookName);
                Webhook.configurationWrapper.appendChild(baseConfig);

                // Load configuration
                Webhook.mqtt.setConfig(config, baseConfig);
            },
            setConfig: function (config, element) {
                Webhook.baseConfig.setConfig(config, element);
                element.querySelector("[data-name=txtMqttServer]").value = config.MqttServer || "";
                element.querySelector("[data-name=txtMqttPort]").value = config.MqttPort || 1883;
                element.querySelector("[data-name=txtTopic]").value = Webhook.atou(config.Topic || "");
                element.querySelector("[data-name=chkUseCredentials]").checked = config.UseCredentials || false;
                element.querySelector("[data-name=txtUsername]").value = config.Username || "";
                element.querySelector("[data-name=txtPassword]").value = config.Password || "";
                element.querySelector("[data-name=chkUseTls]").checked = config.UseTls || false;
                element.querySelector("[data-name=ddlQosLevel]").value = config.QosLevel || "AtMostOnce";
            },
            getConfig: function (element) {
                const config = Webhook.baseConfig.getConfig(element);
                config.MqttServer = element.querySelector("[data-name=txtMqttServer]").value || "";
                config.MqttPort = element.querySelector("[data-name=txtMqttPort]").value || 1883;
                config.Topic = Webhook.utoa(element.querySelector("[data-name=txtTopic]").value || "");
                config.UseCredentials = element.querySelector("[data-name=chkUseCredentials]").checked || false;
                config.Username = element.querySelector("[data-name=txtUsername]").value || "";
                config.Password = element.querySelector("[data-name=txtPassword]").value || "";
                config.UseTls = element.querySelector("[data-name=chkUseTls]").checked || false;
                config.QosLevel = element.querySelector("[data-name=ddlQosLevel]").value || "";
                return config;
            }
        },
        init: async function () {
            // Clear any previously loaded configuration.
            Webhook.configurationWrapper.innerHTML = "";

            // Add click handlers
            Webhook.discord.btnAdd.addEventListener("click", Webhook.discord.addConfig);
            Webhook.generic.btnAdd.addEventListener("click", Webhook.generic.addConfig);
            Webhook.genericForm.btnAdd.addEventListener("click", Webhook.genericForm.addConfig);
            Webhook.gotify.btnAdd.addEventListener("click", Webhook.gotify.addConfig);
            Webhook.pushbullet.btnAdd.addEventListener("click", Webhook.pushbullet.addConfig);
            Webhook.pushover.btnAdd.addEventListener("click", Webhook.pushover.addConfig);
            Webhook.slack.btnAdd.addEventListener("click", Webhook.slack.addConfig);
            Webhook.smtp.btnAdd.addEventListener("click", Webhook.smtp.addConfig);
            Webhook.mqtt.btnAdd.addEventListener("click", Webhook.mqtt.addConfig);
            document.querySelector("#saveConfig").addEventListener("click", Webhook.saveConfig);

            await Webhook.userFilter.populate();
            Webhook.loadConfig();
        },
        removeConfig: function (e) {
            e.preventDefault();
            findParentBySelector(e.target, '[data-config-wrapper]').remove();
        },
        saveConfig: function (e) {
            e.preventDefault();

            Dashboard.showLoadingMsg();

            const config = {};
            config.ServerUrl = document.querySelector("#txtServerUrl").value;
            config.DiscordOptions = [];
            const discordConfigs = document.querySelectorAll("[data-type=discord]");
            for (let i = 0; i < discordConfigs.length; i++) {
                config.DiscordOptions.push(Webhook.discord.getConfig(discordConfigs[i]));
            }

            config.GenericOptions = [];
            const genericConfigs = document.querySelectorAll("[data-type=generic]");
            for (let i = 0; i < genericConfigs.length; i++) {
                config.GenericOptions.push(Webhook.generic.getConfig(genericConfigs[i]));
            }

            config.GenericFormOptions = [];
            const genericFormConfigs = document.querySelectorAll("[data-type=generic-form]");
            for (let i = 0; i < genericFormConfigs.length; i++) {
                config.GenericFormOptions.push(Webhook.genericForm.getConfig(genericFormConfigs[i]));
            }

            config.GotifyOptions = [];
            const gotifyConfigs = document.querySelectorAll("[data-type=gotify]");
            for (let i = 0; i < gotifyConfigs.length; i++) {
                config.GotifyOptions.push(Webhook.gotify.getConfig(gotifyConfigs[i]));
            }

            config.PushbulletOptions = [];
            const pushbulletConfigs = document.querySelectorAll("[data-type=pushbullet]");
            for (let i = 0; i < pushbulletConfigs.length; i++) {
                config.PushbulletOptions.push(Webhook.pushbullet.getConfig(pushbulletConfigs[i]));
            }

            config.PushoverOptions = [];
            const pushoverConfigs = document.querySelectorAll("[data-type=pushover]");
            for (let i = 0; i < pushoverConfigs.length; i++) {
                config.PushoverOptions.push(Webhook.pushover.getConfig(pushoverConfigs[i]));
            }

            config.SlackOptions = [];
            const slackConfigs = document.querySelectorAll("[data-type=slack]");
            for (let i = 0; i < slackConfigs.length; i++) {
                config.SlackOptions.push(Webhook.slack.getConfig(slackConfigs[i]));
            }

            config.SmtpOptions = [];
            const smtpConfigs = document.querySelectorAll("[data-type=smtp]");
            for (let i = 0; i < smtpConfigs.length; i++) {
                config.SmtpOptions.push(Webhook.smtp.getConfig(smtpConfigs[i]));
            }

            config.MqttOptions = [];
            const mqttConfigs = document.querySelectorAll("[data-type=mqtt]");
            for (let i = 0; i < mqttConfigs.length; i++) {
                config.MqttOptions.push(Webhook.mqtt.getConfig(mqttConfigs[i]));
            }

            window.ApiClient.updatePluginConfiguration(Webhook.pluginId, config).then(Dashboard.processPluginConfigurationUpdateResult);
        },
        loadConfig: function () {
            Dashboard.showLoadingMsg();

            window.ApiClient.getPluginConfiguration(Webhook.pluginId).then(function (config) {
                document.querySelector("#txtServerUrl").value = config.ServerUrl || "";
                for (let i = 0; i < config.DiscordOptions.length; i++) {
                    Webhook.discord.addConfig(config.DiscordOptions[i]);
                }

                for (let i = 0; i < config.GenericOptions.length; i++) {
                    Webhook.generic.addConfig(config.GenericOptions[i]);
                }

                for (let i = 0; i < config.GenericFormOptions.length; i++) {
                    Webhook.genericForm.addConfig(config.GenericFormOptions[i]);
                }

                for (let i = 0; i < config.GotifyOptions.length; i++) {
                    Webhook.gotify.addConfig(config.GotifyOptions[i]);
                }

                for (let i = 0; i < config.PushbulletOptions.length; i++) {
                    Webhook.pushbullet.addConfig(config.PushbulletOptions[i]);
                }

                for (let i = 0; i < config.PushoverOptions.length; i++) {
                    Webhook.pushover.addConfig(config.PushoverOptions[i]);
                }

                for (let i = 0; i < config.SlackOptions.length; i++) {
                    Webhook.slack.addConfig(config.SlackOptions[i]);
                }

                for (let i = 0; i < config.SmtpOptions.length; i++) {
                    Webhook.smtp.addConfig(config.SmtpOptions[i]);
                }

                for (let i = 0; i < config.MqttOptions.length; i++) {
                    Webhook.mqtt.addConfig(config.MqttOptions[i]);
                }
            });

            Dashboard.hideLoadingMsg()
        },
        /**
         * ASCII to Unicode (decode Base64 to original data)
         * @param {string} b64
         * @return {string}
         */
        atou: function (b64) {
            return decodeURIComponent(escape(atob(b64)));
        },
        /**
         * Unicode to ASCII (encode data to Base64)
         * @param {string} data
         * @return {string}
         */
        utoa: function (data) {
            return btoa(unescape(encodeURIComponent(data)));
        }
    }

    view.addEventListener("viewshow", async function () {
        await Webhook.init();
    });
}

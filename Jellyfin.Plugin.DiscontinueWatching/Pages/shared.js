/**
 * Shared utilities for Dismiss Watching plugin configuration pages
 */

export const PLUGIN_ID = 'd4af5c47-69e0-47b7-84cc-1b556b76f98a';

// region private variables
let config = undefined;
let users = undefined;
// endregion

// region listeners
const registeredEventListeners = {};
const onConfigLoadedListeners = {};

export const setOnConfigUpdatedListener = (key, listener) => {
  onConfigLoadedListeners[key] = listener;
};

const triggerConfigListeners = value => {
  Object.values(onConfigLoadedListeners).forEach(listener => listener?.(config));
};
// endregion

// region getters/setters
export const getConfig = () => config;

export const setConfig = value => {
  config = value;
  triggerConfigListeners(config);
};

export const getUsers = () => users;
// endregion

// region helpers
export const setPage = resource => {
  const tabs = DismissWatchingTabs();

  const index = tabs.findIndex(tab => tab.resource === resource);

  if (index === -1) {
    console.error(`[Dismiss Watching] Failed to find tab for ${resource}`);
    return;
  }

  console.log(`[Dismiss Watching] ${tabs[index].name} loaded`);

  LibraryMenu.setTabs(tabs[index].resource, index, DismissWatchingTabs);
};

export const loadConfiguration = () => {
  Dashboard.showLoadingMsg();

  return ApiClient.getPluginConfiguration(PLUGIN_ID)
    .then(function (loadedConfig) {
      setConfig(loadedConfig);
      Dashboard.hideLoadingMsg();
      return loadedConfig;
    })
    .catch(function (error) {
      console.error('[Dismiss Watching] Error loading configuration:', error);
      Dashboard.hideLoadingMsg();
      throw error;
    });
};

export const saveConfiguration = newConfig => {
  Dashboard.showLoadingMsg();

  const configToSave = newConfig || config;

  return ApiClient.updatePluginConfiguration(PLUGIN_ID, configToSave)
    .then(function (result) {
      Dashboard.processPluginConfigurationUpdateResult(result);
      setConfig(configToSave);
      return result;
    })
    .catch(function (error) {
      console.error('[Dismiss Watching] Error saving configuration:', error);
      Dashboard.hideLoadingMsg();
      throw error;
    });
};

export const loadUsers = () => {
  return ApiClient.getUsers().then(function (loadedUsers) {
    users = {};
    loadedUsers.forEach(function (user) {
      console.log(`[Dismiss Watching] Loaded user: ${user.Name} (${user.Id})`);
      // add dashes back to guid
      const guidWithDashes = user.Id.replace(/^(.{8})(.{4})(.{4})(.{4})(.{12})$/, '$1-$2-$3-$4-$5');
      users[guidWithDashes] = user.Name;
    });
    return users;
  });
};

// Prevent duplicate listeners from being created every time a tab is switched
export const keyedEventListener = (el, type, listener) => {
  const elId = el.getAttribute('id');

  if (!elId) {
    console.warn('[Dismiss Watching] Element has no id, cannot register keyed listener');
    el.addEventListener(type, listener);
    return;
  }

  if (!registeredEventListeners[elId]) {
    registeredEventListeners[elId] = {
      type,
      listener,
    };
    el.addEventListener(type, listener);
  }
};

export const DismissWatchingTabs = () => [
  {
    href: 'configurationpage?name=Settings',
    resource: 'Settings',
    name: 'Settings',
  },
  {
    href: 'configurationpage?name=UserLists',
    resource: 'UserLists',
    name: 'User Lists',
  },
  {
    href: 'configurationpage?name=Info',
    resource: 'Info',
    name: 'Info',
  },
];
// endregion

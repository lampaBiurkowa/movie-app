import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import axios from 'axios';

import 'bootstrap/dist/css/bootstrap.min.css'
axios.defaults.baseURL = '/api';

const app = createApp(App);

app.mount('#app');
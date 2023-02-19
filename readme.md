# Demo of subscriptions eligibility checker using Camunda, RabbitMQ, Redis & Dapr

Goal of this example is to see how Camunda, RabbitMQ, Redis & Dapr can work together and take advantage of Rabbit connector and Dapr for simplified code.

## [todo]

## Workflow

![subscription-workflow.png](assets%2Fsubscription-workflow.png)

Subscription state is stored into Redis.

MUW - CMS communication is using publish/subscribe pattern and RabbitMQ

### Notes

MacOS/Linux: `export NAMESPACE=demo`

Windows: `setx NAMESPACE "demo"`

### Glossary

sim - Simulation API - runs Cron to trigger requests every x seconds

muw - medical evidence tool API - binds workflow with Camunda

cms - claims management system API - finds client portfolio